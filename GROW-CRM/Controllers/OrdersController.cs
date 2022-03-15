using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using Microsoft.AspNetCore.Http.Features;
using GROW_CRM.Utilities;
using GROW_CRM.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace GROW_CRM.Controllers
{
    public class OrdersController : Controller
    {
        private readonly GROWContext _context;

        public OrdersController(GROWContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string MemberSearch, string VolunteerSearch,
            int? HouseholdID, int? page, int? pageSizeID, string actionButton,
            string sortDirection = "asc", string sortField = "Household")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = ""; //Asume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = " show" if true;

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Household", "Date", "Payment", "Total" };

            PopulateDropDownLists();

            //Start with Includes but make sure your expression returns an
            //IQueryable<Athlete> so we can add filter and sort 
            //options later.
            var orders = from o in _context.Orders
                        .Include(o => o.Member)
                        .Include(o => o.Household)
                        .Include(o => o.PaymentType)
                        .Include(o => o.OrderItems).ThenInclude(i => i.Item)
                        .AsNoTracking()
                         select o;

            //Add as many filters as needed
            if (HouseholdID.HasValue)
            {
                orders = orders.Where(p => p.HouseholdID == HouseholdID);
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(MemberSearch))
            {
                orders = orders.Where(p => p.Member.LastName.ToUpper().Contains(MemberSearch.ToUpper())
                                       || p.Member.FirstName.ToUpper().Contains(MemberSearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(VolunteerSearch))
            {
                orders = orders.Where(p => p.Volunteer.ToUpper().Contains(VolunteerSearch.ToUpper()));
                ViewData["Filtering"] = " show";
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
            //Now we know which field and direction to sort by
            if (sortField == "Household")
            {
                if (sortDirection == "asc")
                {
                    orders = orders
                        .OrderBy(p => p.Household.ID);
                }
                else
                {
                    orders = orders
                        .OrderByDescending(p => p.Household.PostalCode);
                }
            }
            else if (sortField == "Purchase Date")
            {
                if (sortDirection == "asc")
                {
                    orders = orders
                        .OrderByDescending(p => p.Date);
                }
                else
                {
                    orders = orders
                        .OrderBy(p => p.Date);
                }
            }
            else if (sortField == "Payment")
            {
                if (sortDirection == "asc")
                {
                    orders = orders
                        .OrderBy(p => p.PaymentType.Type);
                }
                else
                {
                    orders = orders
                        .OrderByDescending(p => p.PaymentType.Type);
                }
            }
            else //Sorting by Athlete Name
            {
                if (sortDirection == "Total")
                {
                    orders = orders
                        .OrderBy(p => p.Total);
                }
                else
                {
                    orders = orders
                        .OrderByDescending(p => p.Total);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Order>.CreateAsync(orders.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }
        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Member)
                .Include(o => o.PaymentType)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create(int code)
        {
            Order order = new Order { Date = DateTime.Now ,MemberID = code, PaymentTypeID = 1};

            _context.Orders.Add(order);
            _context.SaveChanges();

            var member = (Member)_context.Members.Where(m => m.ID == code).Include(m => m.Household).Select(m => m).FirstOrDefault();

            ViewData["FullName"] = member.FullName;
            ViewData["Address"] = member.Household.FullAddress;
            ViewData["Age"] = member.Age;

            PopulateDropDownLists();
            return View(order);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Date,Subtotal,Taxes,Total,MemberID,PaymentTypeID")] Order order)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            var orderToUpdate = await _context.Orders
                .Include(m => m.Member)
                .Include(m => m.PaymentType)
                .FirstOrDefaultAsync(m => m.ID == order.ID);



            //Check that you got it or exit with a not found error
            if (orderToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Order>(orderToUpdate, "",
                o => o.Date, o => o.Subtotal, o => o.Taxes, o => o.Total, o => o.MemberID, o => o.PaymentTypeID))
            {
                try
                {
                    _context.Update(orderToUpdate);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
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
                    Console.WriteLine("Something Went Wrong");
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            
            PopulateDropDownLists(orderToUpdate);
            return View(orderToUpdate);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                                .Include(o => o.Member)
                                .Include(o => o.PaymentType)
                                .Include(o => o.OrderItems).ThenInclude(oi => oi.Item)
                                .FirstOrDefaultAsync(o => o.ID == id);
            if (order == null)
            {
                return NotFound();
            }

            var member = (Member)_context.Members.Where(m => m.ID == order.MemberID).Include(m => m.Household).Select(m => m).FirstOrDefault();

            ViewData["FullName"] = member.FullName;
            ViewData["Address"] = member.Household.FullAddress;
            ViewData["Age"] = member.Age;

            PopulateDropDownLists(order);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Date,Subtotal,Taxes,Total,MemberID,PaymentTypeID")] Order order)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            var orderToUpdate = await _context.Orders
                .Include(m => m.Member)
                .Include(m => m.PaymentType)
                .FirstOrDefaultAsync(m => m.ID == id);



            //Check that you got it or exit with a not found error
            if (orderToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Order>(orderToUpdate, "",
                o => o.Date, o => o.Subtotal, o => o.Taxes, o => o.Total, o => o.MemberID, o => o.PaymentTypeID))
            {
                try
                {
                    _context.Update(orderToUpdate);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
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
                    Console.WriteLine("Something Went Wrong");
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateDropDownLists(orderToUpdate);
            return View(orderToUpdate);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }

        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }
    }
}
