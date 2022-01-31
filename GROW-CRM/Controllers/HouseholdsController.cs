using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace GROW_CRM.Controllers
{
    public class HouseholdsController : Controller
    {
        private readonly GROWContext _context;

        public HouseholdsController(GROWContext context)
        {
            _context = context;
        }

        // GET: Households
        public async Task<IActionResult> Index()
        {
            var gROWContext = _context.Households.Include(h => h.HouseholdStatus).Include(h => h.Province);
            return View(await gROWContext.ToListAsync());
        }

        // GET: Households/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.HouseholdStatus)
                .Include(h => h.Province)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (household == null)
            {
                return NotFound();
            }

            return View(household);
        }

        // GET: Households/Create
        public IActionResult Create()
        {
            ViewData["HouseholdStatusID"] = new SelectList(_context.HouseholdStatuses, "ID", "ID");
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID");
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StreetNumber,StreetName,AptNumber,City,PostalCode,HouseholdCode,YearlyIncome,NumberOfMembers,LICOVerified,JoinedDate,ProvinceID,HouseholdStatusID")] Household household, List<IFormFile> theFiles)
        {
            if (ModelState.IsValid)
            {
                await AddDocumentsAsync(household, theFiles);
                _context.Add(household);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HouseholdStatusID"] = new SelectList(_context.HouseholdStatuses, "ID", "ID", household.HouseholdStatusID);
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);
            return View(household);
        }

        // GET: Households/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households.FindAsync(id);
            if (household == null)
            {
                return NotFound();
            }
            ViewData["HouseholdStatusID"] = new SelectList(_context.HouseholdStatuses, "ID", "ID", household.HouseholdStatusID);
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,StreetNumber,StreetName,AptNumber,City,PostalCode,HouseholdCode,YearlyIncome,NumberOfMembers,LICOVerified,JoinedDate,ProvinceID,HouseholdStatusID")] Household household)
        {
            if (id != household.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(household);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HouseholdExists(household.ID))
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
            ViewData["HouseholdStatusID"] = new SelectList(_context.HouseholdStatuses, "ID", "ID", household.HouseholdStatusID);
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);
            return View(household);
        }

        // GET: Households/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.HouseholdStatus)
                .Include(h => h.Province)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (household == null)
            {
                return NotFound();
            }

            return View(household);
        }

        // POST: Households/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var household = await _context.Households.FindAsync(id);
            _context.Households.Remove(household);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HouseholdExists(int id)
        {
            return _context.Households.Any(e => e.ID == id);
        }

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.FileContent.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Household household, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    //Note: you could filter for mime types if you only want to allow
                    //certain types of files.  I am allowing everything.
                    if (!(fileName == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        HouseholdDocument d = new HouseholdDocument();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.FileContent.MimeType = mimeType;
                        d.FileName = fileName;
                        household.HouseholdDocuments.Add(d);
                    };
                }
            }
        }


    }
}
