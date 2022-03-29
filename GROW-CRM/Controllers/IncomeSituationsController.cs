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
    [Authorize]
    public class IncomeSituationsController : Controller
    {
        private readonly GROWContext _context;

        public IncomeSituationsController(GROWContext context)
        {
            _context = context;
        }

        // GET: IncomeSituations
        public async Task<IActionResult> Index()
        {
            return View(await _context.IncomeSituations.ToListAsync());
        }

        // GET: IncomeSituations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incomeSituation = await _context.IncomeSituations
                .FirstOrDefaultAsync(m => m.ID == id);
            if (incomeSituation == null)
            {
                return NotFound();
            }

            return View(incomeSituation);
        }

        // GET: IncomeSituations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: IncomeSituations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Situation")] IncomeSituation incomeSituation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(incomeSituation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(incomeSituation);
        }

        // GET: IncomeSituations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incomeSituation = await _context.IncomeSituations.FindAsync(id);
            if (incomeSituation == null)
            {
                return NotFound();
            }
            return View(incomeSituation);
        }

        // POST: IncomeSituations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Situation")] IncomeSituation incomeSituation)
        {
            if (id != incomeSituation.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(incomeSituation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeSituationExists(incomeSituation.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(incomeSituation);
        }

        // GET: IncomeSituations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incomeSituation = await _context.IncomeSituations
                .FirstOrDefaultAsync(m => m.ID == id);
            if (incomeSituation == null)
            {
                return NotFound();
            }

            return View(incomeSituation);
        }

        // POST: IncomeSituations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var incomeSituation = await _context.IncomeSituations.FindAsync(id);
            _context.IncomeSituations.Remove(incomeSituation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IncomeSituationExists(int id)
        {
            return _context.IncomeSituations.Any(e => e.ID == id);
        }
    }
}
