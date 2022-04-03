using GROW_CRM.Data;
using GROW_CRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DietaryRestrictionsController : Controller
    {
        private readonly GROWContext _context;

        public DietaryRestrictionsController(GROWContext context)
        {
            _context = context;
        }

        // GET: DietaryRestrictions
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
        }

        // GET: DietaryRestrictions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dietaryRestriction = await _context.DietaryRestrictions
                .Include(m => m.HealthIssueType)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (dietaryRestriction == null)
            {
                return NotFound();
            }

            return View(dietaryRestriction);
        }

        // GET: DietaryRestrictions/Create
        public IActionResult Create()
        {
            new DietaryRestriction();
            ViewData["HealthIssueTypeID"] = new SelectList(_context.HealthIssueTypes, "ID", "Type");
            PopulateDropDownLists();
            return View();
        }

        // POST: DietaryRestrictions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Restriction,HealthIssueTypeID")] DietaryRestriction dietaryRestriction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dietaryRestriction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
            }
            PopulateDropDownLists(dietaryRestriction);
            return View(dietaryRestriction);
        }

        // GET: DietaryRestrictions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dietaryRestriction = await _context.DietaryRestrictions
                .Include(m => m.HealthIssueType)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (dietaryRestriction == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(dietaryRestriction);
            return View(dietaryRestriction);
        }

        // POST: DietaryRestrictions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Restriction,HealthIssueTypeID")] DietaryRestriction dietaryRestriction)
        {
            if (id != dietaryRestriction.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dietaryRestriction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DietaryRestrictionExists(dietaryRestriction.ID))
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
            PopulateDropDownLists(dietaryRestriction);
            return View(dietaryRestriction);
        }

        // GET: DietaryRestrictions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dietaryRestriction = await _context.DietaryRestrictions
                .Include(m=>m.HealthIssueType)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (dietaryRestriction == null)
            {
                return NotFound();
            }

            return View(dietaryRestriction);
        }

        // POST: DietaryRestrictions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
       {
            var dietaryRestriction = await _context.DietaryRestrictions.FindAsync(id);
            try
            {
                _context.DietaryRestrictions.Remove(dietaryRestriction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });

            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Dietary Restriction. Remember, you cannot delete a Dietary Restriction that has Members associated with.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(dietaryRestriction);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }

        private bool DietaryRestrictionExists(int id)
        {
            return _context.DietaryRestrictions.Any(e => e.ID == id);
        }

        private SelectList HealthIssueTypeSelectList(int? selectedId)
        {
            return new SelectList(_context.HealthIssueTypes
                .OrderBy(d => d.Type), "ID", "Type", selectedId);
        }

        private void PopulateDropDownLists(DietaryRestriction dr = null)
        {
            ViewData["HealthIssueTypeID"] = HealthIssueTypeSelectList(dr?.HealthIssueTypeID);
        }
    }
}
