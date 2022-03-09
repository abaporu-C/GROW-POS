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
                .Include(o => o.Household)
                .Include(o => o.OrderItems).ThenInclude(i => i.Item)
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
        public IActionResult Create()
        {
            ViewDataReturnURL();

            Order order = new Order();
            PopulateDropDownLists(order);
            PopulateSalesItems(order);
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,HouseholdCode,HouseMember,Date,Purchases,Price,Payment,Volunteer,Subtotal,Taxes,Total,MemberID,HouseholdID,PaymentTypeID")] Order order, string[] selectedItemOptions)
        {
            ViewDataReturnURL();

            try
            {
                UpdateOrderItems(selectedItemOptions, order);
                if (ModelState.IsValid)
                {
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { order.ID });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            PopulateSalesItems(order);
            PopulateDropDownLists(order);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(o => o.Item)
                .FirstOrDefaultAsync(o => o.ID == id);
            if (order == null)
            {
                return NotFound();
            }
            PopulateSalesItems(order);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions)
        {
            var orderToUpdate = await _context.Orders
                .Include(d => d.OrderItems)
                .ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (orderToUpdate == null)
            {
                return NotFound();
            }

            UpdateOrderItems(selectedOptions, orderToUpdate);

            if (await TryUpdateModelAsync<Order>(orderToUpdate, "", o => o.HouseholdCode, o => o.HouseMember, o => o.Date, o => o.Purchases, o => o.Price, o => o.Payment, o => o.Volunteer, o => o.Subtotal, o => o.Taxes, o => o.Total))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { orderToUpdate.ID });
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

            PopulateSalesItems(orderToUpdate);
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
                .Include(o => o.Household)
                .Include(o => o.OrderItems).ThenInclude(i => i.Item)
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

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }

        private SelectList HouseholdSelectList(int? selectedId)
        {
            return new SelectList(_context.Households
                .OrderBy(d => d.ID)
                .ThenBy(d => d.Name), "ID", "Name", selectedId);
        }
        private SelectList PaymentSelectList(int? selectedId)
        {
            return new SelectList(_context.PaymentTypes
                .OrderBy(d => d.ID)
                .ThenBy(d => d.Type), "ID", "Type", selectedId);
        }
        private SelectList MemberSelectList(int? HouseholdID, int? selectedId)
        {
            var query = from c in _context.Members.Include(c => c.Household)
                        where c.HouseholdID == HouseholdID.GetValueOrDefault()
                        select c;
            return new SelectList(query.OrderBy(p => p.LastName), "ID", "FullName", selectedId);
        }
        private SelectList ItemSelectList(int? selectedId)
        {
            return new SelectList(_context.Items
                .OrderBy(i => i.ID)
                .ThenBy(i => i.Name), "ID", "Name", selectedId);
        }

        private void PopulateDropDownLists(Order order = null)
        {
            ViewData["HouseholdID"] = HouseholdSelectList(order?.HouseholdID);
            ViewData["MemberID"] = MemberSelectList(order?.HouseholdID, order?.MemberID);
            ViewData["PaymentTypeID"] = PaymentSelectList(order?.PaymentTypeID);
            //ViewData["ItemID"] = ItemSelectList(order?.ItemID);
        }

        private void PopulateSalesItems(Order order)
        {
            var allOptions = _context.Items;
            var currentOptionsHS = new HashSet<int>(order.OrderItems.Select(b => b.ItemID));

            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.Name
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.Name
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }

        private void UpdateOrderItems(string[] selectedOptions, Order orderToUpdate)
        {
            if (selectedOptions == null)
            {
                orderToUpdate.OrderItems = new List<OrderItem>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(orderToUpdate.OrderItems.Select(b => b.ItemID));
            foreach (var i in _context.Items)
            {
                if (selectedOptionsHS.Contains(i.ID.ToString()))
                {
                    if (!currentOptionsHS.Contains(i.ID))
                    {
                        orderToUpdate.OrderItems.Add(new OrderItem
                        {
                            ItemID = i.ID,
                            OrderID = orderToUpdate.ID
                        });
                    }
                }
                else
                {
                    if (currentOptionsHS.Contains(i.ID))
                    {
                        OrderItem itemToRemove = orderToUpdate.OrderItems.FirstOrDefault(o => o.ItemID == i.ID);
                        _context.Remove(itemToRemove);
                    }
                }
            }
        }

        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        public PartialViewResult OrderItemsList(int id)
        {
            ViewBag.OrderItemsList = _context.OrderItems
                .Include(i => i.Item)
                .Where(s => s.OrderID == id)
                .OrderBy(i => i.Item.Name)
                .ToList();
            return PartialView("_OrderItemsList");
        }
    }
}
