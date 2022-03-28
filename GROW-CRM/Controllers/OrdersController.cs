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
using System.Net;

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
            ViewData["Modals"] = new List<string> { "_CreateOrderModal" };
            var gROWContext = _context.Orders.Include(o => o.Member).Include(o => o.PaymentType);
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

            //if(membersDDl == 0) return new ObjectResult("Please, select an existing member.") { StatusCode = 400};

            Order order = new Order { Date = DateTime.Now, Total = 0, MemberID = membersDDl, PaymentTypeID = 1 };

            _context.Orders.Add(order);
            _context.SaveChanges();

            var member = (Member)_context.Members.Where(m => m.ID == membersDDl).Include(m => m.Household).Select(m => m).FirstOrDefault();

            ViewData["FullName"] = member.FullName;
            ViewData["Address"] = member.Household.FullAddress;
            ViewData["Age"] = member.Age;
            ViewData["Modals"] = new List<string> { "_OrderModal" };

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
                                .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.Province)
                                .Include(o => o.Member).ThenInclude(m => m.Household).ThenInclude(h => h.City)
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

        public async Task<ActionResult> GetMembers(int code)
        {
            //Checks to see if there is a valid household ID for the user input
            var householdID = await _context.Households.FirstOrDefaultAsync(h => h.ID == code);
            if (householdID == null) return new ObjectResult("Please, enter a valid Household ID.\nIt must consist only of positive numbers.") { StatusCode = 400 };

            var members = await _context.Members.Where(m => m.HouseholdID == code && m.FirstName != "" && m.LastName != "").ToListAsync();
            
            if (!members.Any()) return new ObjectResult("There are no members registered on this Household ID") { StatusCode = 400};
            return new JsonResult(members);
        }
    }
}
