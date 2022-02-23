using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;

namespace GROW_CRM.Controllers
{
    public class NotificationTypesController : Controller
    {
        private readonly GROWContext _context;

        public NotificationTypesController(GROWContext context)
        {
            _context = context;
        }

        // GET: NotificationTypes
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
        }

        // GET: NotificationTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (notificationType == null)
            {
                return NotFound();
            }

            return View(notificationType);
        }

        // GET: NotificationTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NotificationTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Type")] NotificationType notificationType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notificationType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
            }
            return View(notificationType);
        }

        // GET: NotificationTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes.FindAsync(id);
            if (notificationType == null)
            {
                return NotFound();
            }
            return View(notificationType);
        }

        // POST: NotificationTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Type")] NotificationType notificationType)
        {
            if (id != notificationType.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notificationType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationTypeExists(notificationType.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
            }
            return View(notificationType);
        }

        // GET: NotificationTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (notificationType == null)
            {
                return NotFound();
            }

            return View(notificationType);
        }

        // POST: NotificationTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notificationType = await _context.NotificationTypes.FindAsync(id);
            _context.NotificationTypes.Remove(notificationType);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }

        private bool NotificationTypeExists(int id)
        {
            return _context.NotificationTypes.Any(e => e.ID == id);
        }
    }
}
