using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using Microsoft.AspNetCore.Authorization;

namespace GROW_CRM.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class HealthIssueTypesController : Controller
    {
        private readonly GROWContext _context;

        public HealthIssueTypesController(GROWContext context)
        {
            _context = context;
        }

        // GET: HealthIssueTypes
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
        }

        // GET: HealthIssueTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var healthIssueType = await _context.HealthIssueTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (healthIssueType == null)
            {
                return NotFound();
            }

            return View(healthIssueType);
        }

        // GET: HealthIssueTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HealthIssueTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Type")] HealthIssueType healthIssueType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(healthIssueType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
            }
            return View(healthIssueType);
        }

        // GET: HealthIssueTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var healthIssueType = await _context.HealthIssueTypes.FindAsync(id);
            if (healthIssueType == null)
            {
                return NotFound();
            }
            return View(healthIssueType);
        }

        // POST: HealthIssueTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Type")] HealthIssueType healthIssueType)
        {
            if (id != healthIssueType.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(healthIssueType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HealthIssueTypeExists(healthIssueType.ID))
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
            return View(healthIssueType);
        }

        // GET: HealthIssueTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var healthIssueType = await _context.HealthIssueTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (healthIssueType == null)
            {
                return NotFound();
            }

            return View(healthIssueType);
        }

        // POST: HealthIssueTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var healthIssueType = await _context.HealthIssueTypes.FindAsync(id);
            _context.HealthIssueTypes.Remove(healthIssueType);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }

        private bool HealthIssueTypeExists(int id)
        {
            return _context.HealthIssueTypes.Any(e => e.ID == id);
        }
    }
}
