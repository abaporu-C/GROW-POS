using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ItemsController : Controller
    {
        private readonly GROWContext _context;

        public ItemsController(GROWContext context)
        {
            _context = context;
        }

        // GET: Items
        public async Task<IActionResult> Index(string SearchString, int? CategoryID, bool? ProductPriceStatus,
            string actionButton, int? page, int? pageSizeID, string sortDirection = "asc", string sortField = "Code")
        {
            bool isFiltering = false;

            PopulateDropDownLists();

            string[] sortOptions = new[] { "Code", "Name", "Category", "Discount (%)", "Price ($)" };

            var items = from p in _context
                           .Items
                           .Include(i => i.Category)
                           .AsNoTracking()
                        select p;

            if (CategoryID.HasValue)
            {
                items = items.Where(i => i.CategoryID == CategoryID);
                isFiltering = true;
            }

            if (!String.IsNullOrEmpty(SearchString))
            {
                items = items.Where(i => i.Name.ToUpper().Contains(SearchString.ToUpper()));
                isFiltering = true;
            }

            if (ProductPriceStatus.HasValue && ProductPriceStatus.Value == true)
            {
                items = items.Where(i => i.Discount.HasValue);
                isFiltering = true;
            }

            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;

                if (actionButton != "Filter")
                {
                    if (actionButton == sortField)
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    else
                    {
                        sortDirection = "asc";
                    }
                    sortField = actionButton;
                }
            }


            if (sortField == "Code")
            {
                if (sortDirection == "asc")
                {
                    items = items
                        .OrderBy(p => p.Code);
                }
                else
                {
                    items = items
                        .OrderByDescending(p => p.Code);
                }
            }
            else if (sortField == "Name")
            {
                if (sortDirection == "asc")
                {
                    items = items
                        .OrderBy(p => p.Name);
                }
                else
                {
                    items = items
                        .OrderByDescending(p => p.Name);
                }
            }
            else if (sortField == "Category")
            {
                if (sortDirection == "asc")
                {
                    items = items
                        .OrderBy(p => p.Category);
                }
                else
                {
                    items = items
                        .OrderByDescending(p => p.Category);
                }
            }
            else if (sortField == "Price ($)")
            {
                if (sortDirection == "asc")
                {
                    items = items
                        .OrderBy(p => p.Price * (1 - (p.Discount.HasValue ? p.Discount : 0)));
                }
                else
                {
                    items = items
                        .OrderByDescending(p => p.Price * (1 - (p.Discount.HasValue ? p.Discount : 0)));
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    items = items
                        .OrderBy(p => p.Discount);
                }
                else
                {
                    items = items
                        .OrderByDescending(p => p.Discount);
                }
            }

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            ViewData["Filtering"] = isFiltering ? " show" : "";
            ViewData["Action"] = "/Items";
            ViewData["Modals"] = new List<string> { "_PageSizeModal" };

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Item>.CreateAsync(items.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Code,Name,Description,Price,Discount,CategoryID")] Item item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(item);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Code", "Unable to save changes. Remember, you cannot have duplicate Item Codes.");
                    ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate Item Names.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

                PopulateDropDownLists(item);
            return View(item);
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(item);
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var itemToUpdate = await _context.Items.FirstOrDefaultAsync(p => p.ID == id);

            if (itemToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Item>(itemToUpdate, "",
                i => i.Code, i => i.Name, i => i.Description, i => i.Price, i => i.Discount, i => i.CategoryID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(itemToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Code", "Unable to save changes. Remember, you cannot have duplicate Item Codes.");
                        ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate Item Names.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }

            PopulateDropDownLists(itemToUpdate);
            return View(itemToUpdate);
        }

        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);

            try
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return View(item);
        }


        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.ID == id);
        }

        private void PopulateDropDownLists(Item item = null)
        {
            var dQuery = from d in _context.Categories
                         orderby d.Name
                         select d;

            ViewData["CategoryID"] = new SelectList(dQuery, "ID", "Name", item?.CategoryID);

        }
    }
}
