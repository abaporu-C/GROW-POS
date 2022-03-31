using GROW_CRM.Data;
using GROW_CRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize]
    public class OrderItemsController : Controller
    {
        private readonly GROWContext _context;

        public OrderItemsController(GROWContext context)
        {
            _context = context;
        }

        // GET: OrderItems
        public async Task<IActionResult> Index()
        {
            var gROWContext = _context.OrderItems.Include(o => o.Item).Include(o => o.Order);
            return View(await gROWContext.ToListAsync());
        }

        // GET: OrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Item)
                .Include(o => o.Order)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // GET: OrderItems/Create
        public PartialViewResult CreateOrderItem(int? id)
        {
            var unusedItems = from oi in _context.Items
                                 where !(from p in _context.OrderItems
                                         where p.OrderID == id
                                         select p.ItemID).Contains(oi.ID)
                                 select oi;

            SelectList items = new
                SelectList(unusedItems
                .OrderBy(a => a.Name), "ID", "Name");

            ViewBag.ItemID = items;

            //So we can save it in tne Form
            ViewData["OrderID"] = id.GetValueOrDefault();
            return PartialView("_CreateOrderItem");
        }

        // POST: OrderItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Quantity,OrderID,ItemID")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ItemID"] = new SelectList(_context.Items, "ID", "Code", orderItem.ItemID);
            ViewData["OrderID"] = new SelectList(_context.Orders, "ID", "ID", orderItem.OrderID);
            return View(orderItem);
        }

        // GET: OrderItems/Edit/5
        public PartialViewResult EditOrderItem(int? id)
        {
            //Get the Sponsorship to edit
            var orderItem = _context.OrderItems.Find(id);

            //Use it to help filter the SelectList so you do not offer options
            //that are already taken.
            //This is a classic NOT IN query done in LINQ but also include the current selection
            var unusedSponsors = from sp in _context.Items
                                 where !(from p in _context.OrderItems
                                         where p.OrderID == orderItem.OrderID
                                         select p.ItemID).Contains(sp.ID)
                                    || sp.ID == orderItem.ItemID //Add in the current sponsor
                                 select sp;

            ViewData["ItemID"] = new
                SelectList(unusedSponsors
                .OrderBy(a => a.Name), "ID", "Name", orderItem.ItemID);

            return PartialView("_EditOrderItem", orderItem);
        }

        // POST: OrderItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            OrderItem orderItemToUpdate = await _context.OrderItems.FindAsync(id);
            if (await TryUpdateModelAsync<OrderItem>(orderItemToUpdate, "",
                p => p.ItemID, p => p.Quantity))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemExists(orderItemToUpdate.ID))
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
            return View(orderItemToUpdate);
        }

        // GET: OrderItems/Delete/5
        public PartialViewResult DeleteOrderItem(int? id)
        {
            OrderItem sponsorship = _context.OrderItems
                .Include(p => p.Item)
                .Where(p => p.ID == id)
                .FirstOrDefault();

            return PartialView("_DeleteOrderItem", sponsorship);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sponsorship = await _context.OrderItems.FindAsync(id);
            try
            {
                _context.OrderItems.Remove(sponsorship);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(sponsorship);
        }



        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.ID == id);
        }
    }
}
