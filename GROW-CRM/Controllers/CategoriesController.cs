using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GROW_CRM.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CategoriesController : Controller
    {
        private readonly GROWContext _context;

        public CategoriesController(GROWContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
        }
        /*// GET: Categories
        public async Task<IActionResult> Index(string SearchString, int? page, int? pageSizeID, string sortDirection = "desc")
        {
            bool isFiltering = false;
            page = page ?? 1;
            sortDirection = sortDirection == "asc" ? "desc" : "asc";

            var categories = from p in _context
                           .Categories
                           .Include(i => i.Items)
                           .AsNoTracking()
                             select p;

            if (!String.IsNullOrEmpty(SearchString))
            {
                categories = categories.Where(i => i.Name.ToUpper().Contains(SearchString.ToUpper()));
                isFiltering = true;
            }

            if (sortDirection == "asc")
            {
                categories = categories
                    .OrderBy(p => p.Name);
            }
            else
            {
                categories = categories
                    .OrderByDescending(p => p.Name);
            }

            ViewData["sortDirection"] = sortDirection;
            ViewData["Filtering"] = isFiltering ? " show" : "";
            ViewData["Action"] = "/Categories";
            ViewData["Modals"] = new List<string> { "_PageSizeModal" };

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Category>.CreateAsync(categories.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }*/

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            Category categoryToUpdate = await _context.Categories.FirstOrDefaultAsync(i => i.ID == id);
            if (categoryToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Category>(categoryToUpdate, "", i => i.Name))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(categoryToUpdate.ID))
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

            return View(categoryToUpdate);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Category. Remember, you cannot delete a Category that has items assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(category);

        }



        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }


        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.ID == id);
        }
    }
}
