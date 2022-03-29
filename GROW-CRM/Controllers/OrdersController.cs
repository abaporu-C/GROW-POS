using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
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
        public async Task<IActionResult> Index()
        {
            var gROWContext = _context.Orders
                .Include(o => o.Member).ThenInclude(o => o.Household)
                .Include(o => o.PaymentType);
            return View(await gROWContext.ToListAsync());
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

            Order order = new Order { Date = DateTime.Today.Date, MemberID = membersDDl, PaymentTypeID = 1 };

            var member = (Member)_context.Members.Where(m => m.ID == membersDDl)
                .Include(m => m.Household).ThenInclude(m => m.Province)
                .Include(m => m.Household).ThenInclude(m => m.City)
                .Select(m => m).FirstOrDefault();

            _context.Orders.Add(order);
            _context.SaveChanges();

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
                .Include(m => m.Member).ThenInclude(m => m.Household)
                .Include(m => m.Member.Household.Province)
                .Include(m => m.Member.Household.City)
                .Include(m => m.PaymentType)
                .Include(m => m.OrderItems).ThenInclude(m => m.Item)
                .FirstOrDefaultAsync(m => m.ID == order.ID);

            var orderItemsCheck = await _context.OrderItems.Where(oi => oi.OrderID == order.ID).ToListAsync();

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
                    //Dave gave me this code to delete empty orders automatically that are older than a day old
                    //var yesterday = DateTime.Today.AddDays(-1);
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
                        ViewBag.ErrorMessage = "You need to add items to this order.";
                        //throw new Exception("You need to add items to this order.");
                    }
                    //else if(orderToUpdate.Total == 0)
                    //{
                    //    //Delete the created record (if there is no total, then the order is empty)
                    //}
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
            ViewDataReturnURL();

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                                .Include(o => o.Member).ThenInclude(o => o.Household)
                                .Include(o => o.PaymentType)
                                .Include(o => o.Member.Household.Province)
                                .Include(o => o.Member.Household.City)
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
                .Include(o => o.Member.Household.Province)
                .Include(o => o.Member.Household.City)
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

        public JsonResult GetMembers(int code)
        {
            var members = _context.Members.Where(m => m.HouseholdID == code).ToList();
            return Json(members);
        }
    }
}
