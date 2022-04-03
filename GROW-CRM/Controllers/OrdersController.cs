using DinkToPdf;
using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly GROWContext _context;

        public OrdersController(GROWContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string MemberSearch, string HouseholdNameSearch, int? HouseholdID,
            int? PaymentTypeID, int? HouseholdIDSearch, int? Total,
            int? page, int? pageSizeID, string actionButton,
            string sortDirection = "asc", string sortField = "Code")
        {
            bool isFiltering = false;

            PopulateDropDownLists();
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Date", "Total", "Payment", "Member" };

            var orders = from o in _context.Orders
                               .Include(o => o.Member).ThenInclude(m => m.Household)
                               .Include(o => o.PaymentType)
                               .AsNoTracking()
                         select o;
            //Add as many filters as needed
            if (HouseholdID.HasValue)
            {
                orders = orders.Where(o => o.Member.HouseholdID == HouseholdID);
                isFiltering = true;
            }
            if (Total.HasValue)
            {
                if (Total < 20)
                    orders = orders.Where(o => o.Total < 20);
                isFiltering = true;
            }
            if (PaymentTypeID.HasValue)
            {
                orders = orders.Where(o => o.PaymentTypeID == PaymentTypeID);
                isFiltering = true;
            }

            if (!String.IsNullOrEmpty(MemberSearch))
            {
                orders = orders.Where(o => o.Member.LastName.ToUpper().Contains(MemberSearch.ToUpper())
                                       || o.Member.FirstName.ToUpper().Contains(MemberSearch.ToUpper()));
                isFiltering = true;
            }
            if (!String.IsNullOrEmpty(HouseholdNameSearch))
            {
                orders = orders.Where(o => o.Member.Household.Name.ToUpper().Contains(HouseholdNameSearch.ToUpper()));
                isFiltering = true;
            }
            if (HouseholdIDSearch != null)
            {
                //TODO validation. Display warning if not integer entered.
                try
                {
                    orders = orders.Where(o => o.ID.Equals(HouseholdIDSearch));
                    isFiltering = true;
                }
                catch (Exception)
                {

                    //TODO
                }
            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }


            if (sortField == "Member")
            {
                if (sortDirection == "asc")
                {
                    orders = orders
                    .OrderBy(o => o.Member.LastName)
                    .ThenBy(o => o.Member.FirstName);
                }
                else
                {
                    orders = orders
                   .OrderByDescending(o => o.Member.LastName)
                   .ThenByDescending(o => o.Member.FirstName);
                }
            }
            else if (sortField == "Date")
            {
                if (sortDirection == "asc")
                {
                    orders = orders
                    .OrderBy(o => o.Date)
                    .ThenBy(h => h.Member.LastName)
                    .ThenBy(o => o.Member.FirstName);
                }
                else
                {
                    orders = orders
                     .OrderByDescending(o => o.Date)
                    .ThenByDescending(h => h.Member.LastName)
                    .ThenByDescending(o => o.Member.FirstName);
                }
            }
            else if (sortField == "Total")
            {
                if (sortDirection == "asc")
                {
                    orders = orders
                   .OrderBy(o => o.Total)
                   .ThenBy(h => h.Member.LastName)
                   .ThenBy(o => o.Member.FirstName);
                }
                else
                {
                    orders = orders
                      .OrderByDescending(o => o.Total)
                     .ThenByDescending(h => h.Member.LastName)
                     .ThenByDescending(o => o.Member.FirstName);
                }
            }
            else if (sortField == "Payment")
            {
                if (sortDirection == "asc")
                {
                    orders = orders
                   .OrderBy(o => o.PaymentType)
                   .ThenBy(h => h.Member.LastName)
                   .ThenBy(o => o.Member.FirstName);
                }
                else
                {
                    orders = orders
                      .OrderByDescending(o => o.PaymentType)
                     .ThenByDescending(h => h.Member.LastName)
                     .ThenByDescending(o => o.Member.FirstName);
                }
            }


            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            ViewData["Filtering"] = isFiltering ? " show" : "";
            ViewData["Action"] = "/Orders";
            ViewData["Modals"] = new List<string> { "_PageSizeModal", "_CreateOrderModal" };

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Order>.CreateAsync(orders.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(o => o.Item)
                .Include(o => o.Member).ThenInclude(o => o.Household)
                .Include(o => o.Member.Household.Province)
                .Include(o => o.Member.Household.City)
                .Include(o => o.PaymentType)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create(int membersDDl)
        {
            ViewDataReturnURL();

            //if(membersDDl == 0) return new ObjectResult("Please, select an existing member.") { StatusCode = 400};

            Order order = new Order { Date = DateTime.Now, Total = 0, MemberID = membersDDl, PaymentTypeID = 1 };

            _context.Orders.Add(order);
            _context.SaveChanges();
           
            ViewData["Modals"] = new List<string> { "_OrderModal" };

            Order viewOrder = _context.Orders
                              .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.City)
                              .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.Province)
                              .Where(m => m.ID == order.ID)
                              .FirstOrDefault();

            PopulateDropDownLists();
            return View(order);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Date,Total,MemberID,PaymentTypeID")] Order order)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            var orderToUpdate = await _context.Orders
                                .Include(o => o.PaymentType)
                                .Include(o => o.OrderItems).ThenInclude(oi => oi.Item)
                                .FirstOrDefaultAsync(o => o.ID == order.ID);

            var orderItemsCheck = await _context.OrderItems.Where(oi => oi.OrderID == order.ID).ToListAsync();

            ViewData["Modals"] = new List<string> { "_OrderModal" };

            //Check that you got it or exit with a not found error
            if (orderToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (ModelState.IsValid && await TryUpdateModelAsync<Order>(orderToUpdate, "",
                o => o.Date, o => o.Total, o => o.MemberID, o => o.PaymentTypeID))
            {
                try
                {
                    var yesterday = order.Date.AddDays(-1);
                    var noItems = (orderItemsCheck == null);
                    var badOrders = await _context.Orders.Where(o => o.Date <= yesterday && noItems).ToListAsync();
                    if (badOrders != null)
                    {
                        _context.Orders.RemoveRange(badOrders);
                        await _context.SaveChangesAsync();
                    }
                    if (!orderItemsCheck.Any())
                    {
                        throw new Exception("You need to add items to this order.");
                    }
                    else
                    {
                        _context.Update(orderToUpdate);
                        await _context.SaveChangesAsync();                        
                        return RedirectToAction("Details", new { order.ID });
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(orderToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            PopulateDropDownLists(orderToUpdate);
            return View(orderToUpdate);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewDataReturnURL();

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                                .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.Province)
                                .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.City)
                                .Include(o => o.PaymentType)
                                .Include(o => o.OrderItems).ThenInclude(oi => oi.Item)
                                .FirstOrDefaultAsync(o => o.ID == id);
            if (order == null)
            {
                return NotFound();
            }

            //var member = (Member)_context.Members.Where(m => m.ID == order.MemberID).Include(m => m.Household).Select(m => m).FirstOrDefault();

            //ViewData["Household"] = member.Household.Name;
            //ViewData["Address"] = member.Household.FullAddress;
            //ViewData["Date"] = DateTime.Today;

            ViewData["Modals"] = new List<string> { "_OrderModal" };

            PopulateDropDownLists(order);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Date,Total,MemberID,PaymentTypeID")] Order order)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            var orderToUpdate = await _context.Orders
                                .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.Province)
                                .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.City)
                                .Include(o => o.PaymentType)
                                .Include(o => o.OrderItems).ThenInclude(oi => oi.Item)
                                .FirstOrDefaultAsync(o => o.ID == id);



            //Check that you got it or exit with a not found error
            if (orderToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (ModelState.IsValid && await TryUpdateModelAsync<Order>(orderToUpdate, "",
                o => o.Date, o => o.Total, o => o.MemberID, o => o.PaymentTypeID))
            {
                try
                {
                    _context.Update(orderToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { order.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(orderToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {                    
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateDropDownLists(orderToUpdate);
            return View(orderToUpdate);
        }

        // GET: Orders/Delete/5
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Member.Household.Province)
                .Include(o => o.Member.Household.City)
                .Include(o => o.OrderItems).ThenInclude(o => o.Item)
                .Include(o => o.Member)
                .Include(o => o.PaymentType)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();

            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.ID == id);
        }

        private SelectList PaymentTypeSelectList(int? id)
        {
            var ptQuery = from pt in _context.PaymentTypes
                          orderby pt.Type
                          select pt;
            return new SelectList(ptQuery, "ID", "Type", id);
        }
        public SelectList MemberSelectList(int? selectedId)
        {
            return new SelectList(_context.Members
                .OrderBy(d => d.LastName).ThenByDescending(d => d.FirstName), "ID", "FullName", selectedId);
        }

        public PartialViewResult OrderItemList(int id)
        {
            ViewBag.OrderItems = _context.OrderItems
                .Include(s => s.Item)
                .Where(s => s.OrderID == id)
                .OrderBy(s => s.Item.Name)
                .ToList();
            return PartialView("_OrderItemLists");
        }

        private void PopulateDropDownLists(Order order = null)
        {
            ViewData["PaymentTypeID"] = PaymentTypeSelectList(order?.PaymentTypeID);
            ViewData["MemberID"] = MemberSelectList(order?.MemberID);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }

        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        public async Task<ActionResult> GetMembers(int code)
        {
            //Checks to see if there is a valid household ID for the user input
            var householdID = await _context.Households.FirstOrDefaultAsync(h => h.ID == code);
            if (householdID == null) return new ObjectResult("Please, enter a valid Household ID.\nIt must consist only of positive numbers.") { StatusCode = 400 };

            var members = await _context.Members.Where(m => m.HouseholdID == code && m.FirstName != "" && m.LastName != "").ToListAsync();
            
            if (!members.Any()) return new ObjectResult("There are no members registered on this Household ID") { StatusCode = 400};
            return new JsonResult(members);
        }

        public IActionResult GetOrderPdf(int id)
        {
            var converter = new SynchronizedConverter(new PdfTools());

            var order = _context.Orders
                        .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.City)
                        .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.Province)
                        .Include(o => o.OrderItems).ThenInclude(oi => oi.Item)
                        .Where(o => o.ID == id).FirstOrDefault();

            string html = @$"
<h1>Order #:{id}</h1>
<hr />

    <dl>
        <dt>
            <b>Member:</b> {order.Member.FullName}
        </dt>
        <dt>
            <b>Address:</b> {order.Member.Household.FullAddress}
        </dt>
        <dt>
            <b>Date:</b> {order.DateFormatted}
        </dt>
    </dl>
    <table>
        <thead>
            <tr>
                <th>
                    Item
                </th>
                <th>
                    Quantity
                </th>
                <th>
                    Price
                </th>
            </tr>            
        </thead>
        <tbody>
            ";

            foreach(OrderItem oi in order.OrderItems)
            {
                html += $@"<tr><td>
                    {oi.Item.Name}
                </td>
                <td>
                    {oi.Quantity}                    
                </td>
                <td>
                    {oi.Item.Price.ToString("C")}
                </td></tr>";
            }

            html += @$"
        </tbody>
<tfoot>
    <tr>
    <td></td>
    <td>Total</td>
    <td>{order.TotalFormatted}</td>
</tr>
</tfoot>
    </table>";

    html += @"<style>th, td {
  border-bottom: 1px solid #ddd;
}
table {
    text-align: center;
    width: 80%;
}
</style>"; 


            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4Plus,
            },
                Objects = {
                    new ObjectSettings() {
                    PagesCount = true,
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                    }
                }
            };

            var result = converter.Convert(doc);

            return File(result,
            "application/octet-stream", $"Order{id}Receit.pdf");
        }
    }
}
