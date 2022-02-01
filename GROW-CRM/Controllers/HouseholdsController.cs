﻿using System;
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
        public async Task<IActionResult> Index( string StreetSearch, string CitySearch, string CodeSearch,
            string HouseholdCodeSearch,
            int? HouseholdID, int? HouseholdStatusID,
            int? page, int? pageSizeID, string actionButton,
            string sortDirection = "asc", string sortField = "Code")
        {
            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = ""; //Asume not filtering

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Code","Street", "City", "Province", "Members", "LICO", "Status" };

            PopulateDropDownLists();



            var households = from h in _context.Households
                                .Include(h => h.HouseholdStatus)
                                .Include(h => h.Province)

                             select h;

            //Add as many filters as needed
            if (HouseholdStatusID.HasValue)
            {
                households = households.Where(h => h.HouseholdStatusID == HouseholdStatusID);
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(StreetSearch))
            {
                households = households.Where(h => h.StreetName.ToUpper().Contains(StreetSearch.ToUpper())
                                       || h.City.ToUpper().Contains(StreetSearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(CitySearch))
            {
                households = households.Where(h => h.StreetName.ToUpper().Contains(CitySearch.ToUpper())
                                       || h.City.ToUpper().Contains(CitySearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(CodeSearch))
            {
                households = households.Where(p => p.HouseholdCode.Contains(CodeSearch));
                ViewData["Filtering"] = " show";
            }


            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }


            if (sortField == "Code")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.HouseholdCode);
                }
                else
                {
                    households = households
                   .OrderByDescending(h => h.HouseholdCode);
                }
            }
            else if (sortField == "Street")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.StreetName)
                    .ThenBy(h => h.StreetNumber);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.StreetName)
                    .ThenByDescending(h => h.StreetNumber);
                }
            }
            else if (sortField == "City")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.City)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.City)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Province")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.Province.Name)
                    .ThenBy(h => h.City)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                    .OrderByDescending(h => h.Province.Name)
                    .ThenByDescending(h => h.City)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Members")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.NumberOfMembers)
                    .ThenBy(h => h.City);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.NumberOfMembers)
                    .ThenByDescending(h => h.City);
                }
            }
            else if (sortField == "LICO")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.LICOVerified)
                    .ThenBy(h => h.City)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.LICOVerified)
                    .ThenByDescending(h => h.City)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.HouseholdStatus.Name)
                    .ThenBy(h => h.City)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.HouseholdStatus.Name)
                    .ThenByDescending(h => h.City)
                    .ThenByDescending(h => h.StreetName);
                }
            }



            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            return View(await households.ToListAsync());
        }

        // GET: Households/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.Members).ThenInclude(m => m.IncomeSituation)
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
            var household = new Household();
            
            PopulateDropDownLists(household);
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StreetNumber,StreetName,AptNumber,City,PostalCode,HouseholdCode,YearlyIncome,NumberOfMembers,LICOVerified,JoinedDate,ProvinceID,HouseholdStatusID")] Household household, List<IFormFile> theFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(household, theFiles);
                    _context.Add(household);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { household.ID });
                }
            }
            catch (DbUpdateException)
            {

                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
           
           /* ViewData["HouseholdStatusID"] = new SelectList(_context.HouseholdStatuses, "ID", "ID", household.HouseholdStatusID);
            ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);*/
            return View(household);
        }

        // GET: Households/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.HouseholdStatus)
                .Include(h => h.Province)
                .FirstOrDefaultAsync(h => h.ID == id);

            if (household == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(household);
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {

            //Go get the Household to update

            var householdToUpdate = await _context.Households
                .Include(h => h.Province)
                .Include(h => h.HouseholdStatus)
                .SingleOrDefaultAsync(h => h.ID == id);

            //Check that you got it or exit with a not found error
            if (householdToUpdate == null)
            {
                return NotFound();
            }

            

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Household>(householdToUpdate, "",
                h => h.StreetName, h => h.StreetNumber, h => h.AptNumber, h => h.City, h => h.PostalCode,
                h => h.HouseholdCode, h => h.YearlyIncome, h => h.LICOVerified, h => h.NumberOfMembers, h => h.JoinedDate,
                h => h.ProvinceID, h => h.HouseholdStatusID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { householdToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HouseholdExists(householdToUpdate.ID))
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


            PopulateDropDownLists(householdToUpdate);
            return View(householdToUpdate);
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
        private SelectList ProvinceSelectList(int? selectedId)
        {
            return new SelectList(_context.Provinces
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private SelectList HouseholdStatusSelectList(int? selectedId)
        {
            return new SelectList(_context.HouseholdStatuses
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(Household household = null)
        {
            ViewData["ProvinceID"] = ProvinceSelectList(household?.ProvinceID);
            ViewData["HouseholdStatusID"] = HouseholdStatusSelectList(household?.HouseholdStatusID);
           
        }
    }
}
