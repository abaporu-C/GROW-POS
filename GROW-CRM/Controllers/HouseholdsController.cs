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
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authorization;
using GROW_CRM.Utilities;

namespace GROW_CRM.Controllers
{
    [Authorize]
    public class HouseholdsController : Controller
    {
        private readonly GROWContext _context;

        public HouseholdsController(GROWContext context)
        {
            _context = context;
        }

        // GET: Households
        public async Task<IActionResult> Index(string StreetSearch, string CitySearch, string HouseholdNameSearch, int? IDSearch, string sortDirectionCheck, string sortFieldID,
            int? HouseholdID, int? HouseholdStatusID, int? CityID,
            int? page, int? pageSizeID, string actionButton,
            string sortDirection = "asc", string sortField = "Code")
        {
            /*//Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-secondary"; //Asume not filtering*/
            bool isFiltering = false;

            PopulateDropDownLists();

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "ID", "House Name", "City", "Province", "Members", "LICO", "Status" };

            //Trying to save the world

            var households = from h in _context.Households
                                .Include(h => h.HouseholdStatus)
                                .Include(h => h.City)
                                .Include(h => h.Province)
                                .Include(h => h.Members)
                                .AsNoTracking()
                             select h;

            //Add as many filters as needed
            if (HouseholdStatusID.HasValue)
            {
                households = households.Where(h => h.HouseholdStatusID == HouseholdStatusID);
                isFiltering = true;
            }
            if (CityID.HasValue)
            {
                households = households.Where(h => h.CityID == CityID);
                isFiltering = true;
            }
            if (!String.IsNullOrEmpty(HouseholdNameSearch))
            {
                households = households.Where(h => h.Name.ToUpper().Contains(HouseholdNameSearch.ToString().ToUpper()));

                isFiltering = true;
            }
            if (!String.IsNullOrEmpty(StreetSearch))
            {
                households = households.Where(h => h.StreetNumber.Contains(StreetSearch.ToString().ToUpper())
                || (h.StreetNumber + ' ' + h.StreetName.ToUpper()).Contains(StreetSearch.ToString().ToUpper())
                || (h.StreetNumber + ' ' + h.StreetName.ToUpper() + ' ' + h.City.Name.ToUpper()).Contains(StreetSearch.ToString().ToUpper())
                || h.StreetName.ToUpper().Contains(StreetSearch.ToUpper())
                || h.City.Name.ToUpper().Contains(StreetSearch.ToUpper()));

                isFiltering = true;
            }
            if (!String.IsNullOrEmpty(CitySearch))
            {
                households = households.Where(h => h.StreetName.ToUpper().Contains(CitySearch.ToUpper())
                                       || h.City.Name.ToUpper().Contains(CitySearch.ToUpper()));
                isFiltering = true;
            }
            if (IDSearch != null)
            {
                //TODO validation. Display warning if not integer entered.
                try
                {
                    households = households.Where(h => h.ID.Equals(IDSearch));
                    isFiltering = true;
                }
                catch (Exception)
                {

                    //TODO
                }

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


            if (sortField == "ID")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.ID);
                }
                else
                {
                    households = households
                   .OrderByDescending(h => h.ID);
                }
            }
            else if (sortField == "House Name")
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
                    .OrderBy(h => h.City.Name)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.City.Name)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Province")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.Province.Name)
                    .ThenBy(h => h.City.Name)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                    .OrderByDescending(h => h.Province.Name)
                    .ThenByDescending(h => h.City.Name)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Members")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.NumberOfMembers)
                    .ThenBy(h => h.City.Name);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.NumberOfMembers)
                    .ThenByDescending(h => h.City.Name);
                }
            }
            else if (sortField == "LICO")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.LICOVerified)
                    .ThenBy(h => h.City.Name)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.LICOVerified)
                    .ThenByDescending(h => h.City.Name)
                    .ThenByDescending(h => h.StreetName);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    households = households
                    .OrderBy(h => h.HouseholdStatus.Name)
                    .ThenBy(h => h.City.Name)
                    .ThenBy(h => h.StreetName);
                }
                else
                {
                    households = households
                     .OrderByDescending(h => h.HouseholdStatus.Name)
                    .ThenByDescending(h => h.City.Name)
                    .ThenByDescending(h => h.StreetName);
                }
            }



            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            ViewData["Filtering"] = isFiltering ? " show" : "";
            ViewData["Action"] = "/Households";
            ViewData["Modals"] = new List<string> { "_PageSizeModal" };

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Household>.CreateAsync(households.AsNoTracking(), page ?? 1, pageSize);

            //selectlist for Sorting options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());



            return View(pagedData);
        }

        // GET: Households/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.Members).ThenInclude(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
                .Include(h => h.HouseholdStatus)
                .Include(h => h.City)
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
            //var household = new Household();

            var about = (About)_context.Abouts.Where(m => m.ID == 1).FirstOrDefault();

            ViewData["AptNumber"] = about.AptNumber;
            ViewData["StreetNumber"] = about.StreetNumber;
            ViewData["StreetName"] = about.StreetName;
            ViewData["PostalCode"] = about.PostalCode;
            ViewData["CityID"] = about.CityID;
            ViewData["ProvinceID"] = about.ProvinceID;


            PopulateDropDownLists();
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,StreetNumber,StreetName,AptNumber,PostalCode,LICOVerified,LastVerification,CityID,ProvinceID,HouseholdStatusID")] Household household, List<IFormFile> theFiles, string NewID, int? AboutID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(household, theFiles);
                    //See if we can force the NewID
                    if (!String.IsNullOrEmpty(NewID))
                    {
                        if (!Int32.TryParse(NewID, out int newID))
                        {
                            throw new DbUpdateException("Invalid ID");
                        }
                        if (HouseholdExists(newID))
                        {
                            throw new DbUpdateException("ID already used");
                        }
                        else //it is not is use so we can use the requested ID value
                        {
                            //Note that this works for sqLite.  However, with SQL Server
                            //you would need to find a different work around such as generating
                            //new values in a trigger or setting IDENTITY_INSERT in a tranascation.
                            household.ID = newID;
                        }
                    }
                    //Create default household name if Name is empty
                    if (household.Name == null || household.Name == "")
                    {
                        int lastID = _context.Households.ToList().Last().ID;
                        household.Name = $"House #{lastID + 1}";
                    }
                    //if house has custom name, prepend "House" 
                    if (!String.IsNullOrEmpty(household.Name))
                    {
                        household.Name = $"House {household.Name}";
                    }

                    _context.Add(household);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Add", "HouseholdMembers", new { HouseholdID = household.ID, HouseholdName = household.Name });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                string msg = dex.GetBaseException().Message;
                if (msg.Contains("ID already used"))
                {
                    ModelState.AddModelError("", "Another record is already using ID: " + NewID);
                }
                else if (msg.Contains("Invalid ID"))
                {
                    ModelState.AddModelError("", "Invalid ID: the ID must be numeric.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            /*  catch (DbUpdateException)
              {

                  ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
              }*/

            /* ViewData["HouseholdStatusID"] = new SelectList(_context.HouseholdStatuses, "ID", "ID", household.HouseholdStatusID);
             ViewData["ProvinceID"] = new SelectList(_context.Provinces, "ID", "ID", household.ProvinceID);*/


            var about = (About)_context.Abouts.Where(m => m.ID == 1).FirstOrDefault();

            ViewData["AptNumber"] = about.AptNumber;
            ViewData["StreetNumber"] = about.StreetNumber;
            ViewData["StreetName"] = about.StreetName;
            ViewData["PostalCode"] = about.PostalCode;
            ViewData["CityID"] = about.CityID;
            ViewData["ProvinceID"] = about.ProvinceID;


            PopulateDropDownLists(household);

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
                .Include(h => h.City)
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
                .Include(h => h.City)
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

                h => h.Name,
                h => h.StreetName, h => h.StreetNumber, h => h.AptNumber, h => h.PostalCode, h => h.CityID,
                h => h.ProvinceID, h => h.HouseholdStatusID))
            {
                try
                {

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "HouseholdMembers", new { HouseholdID = householdToUpdate.ID });
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
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var household = await _context.Households
                .Include(h => h.HouseholdStatus)
                .Include(h => h.City)
                .Include(h => h.Province)
                .Include(h => h.Members).ThenInclude(m => m.MemberIncomeSituations)
                .FirstOrDefaultAsync(m => m.ID == id);

            var members = from m in _context.Members
                                  .Include(m => m.Gender)
                                  .Include(m => m.Household)
                                  .Include(m => m.MemberDocuments)
                                  .Include(m => m.MemberIncomeSituations)
                          where m.HouseholdID == id && m.FirstName != "" && m.LastName != ""
                          select m;

            List<List<MemberIncomeSituation>> misList = new List<List<MemberIncomeSituation>>();

            foreach (Member m in members)
            {
                var v = _context.MemberIncomeSituations
                .Include(s => s.IncomeSituation)
                .Where(s => s.MemberID == m.ID)
                .OrderBy(s => s.IncomeSituation.Situation)
                .ToList();

                misList.Add(v);
            }

            ViewBag.MisList = misList;

            if (household == null)
            {
                return NotFound();
            }

            return View(household);
        }

        // POST: Households/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var household = await _context.Households.FindAsync(id);
            try
            {
                _context.Households.Remove(household);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Delete All Members before deleting Household.");
            }

            return RedirectToAction("Delete", id);
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
                        MemberDocument d = new MemberDocument();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.FileContent.MimeType = mimeType;
                        d.FileName = fileName;
                        //household.HouseholdDocuments.Add(d);
                    };
                }
            }
        }
        private SelectList CitySelectList(int? selectedId)
        {
            return new SelectList(_context.Cities
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }

        private SelectList ProvinceSelectList(int? selectedId)
        {
            return new SelectList(_context.Provinces

                .OrderByDescending(d => d.Name == "Ontario")
                .ThenBy(d => d.Name), "ID", "Name", selectedId);
        }
        private SelectList HouseholdStatusSelectList(int? selectedId)
        {
            return new SelectList(_context.HouseholdStatuses
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(Household household = null)
        {
            ViewData["CityID"] = CitySelectList(household?.CityID);
            ViewData["ProvinceID"] = ProvinceSelectList(household?.ProvinceID);
            ViewData["HouseholdStatusID"] = HouseholdStatusSelectList(household?.HouseholdStatusID);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());

        }
    }
}
