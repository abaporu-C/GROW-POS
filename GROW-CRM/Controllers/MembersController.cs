using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly GROWContext _context;

        public MembersController(GROWContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index(string MemberSearch, string PhoneSearch, string HouseholdSearch, string sortDirectionCheck, string sortFieldID,
            int? HouseholdIDSearch,
            int? HouseholdID, int? GenderID, 
            int? page, int? pageSizeID, string actionButton,
            string sortDirection = "asc", string sortField = "Member")
        {
            

            

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Member", "Age", "Gender", "Household", "Situation" };

            PopulateDropDownLists();

            var members = from m in _context.Members
                              .Include(m => m.Gender)
                              .Include(m => m.Household).ThenInclude(h => h.City)
                              .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
                          where m.FirstName != "" && m.LastName != ""
                          select m;

            //Add as many filters as needed
            if (HouseholdID.HasValue)
            {
                members = members.Where(p => p.HouseholdID == HouseholdID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (GenderID.HasValue)
            {
                members = members.Where(p => p.GenderID == GenderID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(MemberSearch))
            {
                members = members.Where(p => p.LastName.ToUpper().Contains(MemberSearch.ToUpper())
                                       || p.FirstName.ToUpper().Contains(MemberSearch.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(PhoneSearch))
            {
                members = members.Where(p => p.PhoneNumber.Contains(PhoneSearch));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(HouseholdSearch))
            {
                members = members.Where(p => p.Household.StreetName.ToUpper().Contains(HouseholdSearch.ToUpper())
                                       || p.Household.City.Name.ToUpper().Contains(HouseholdSearch.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(HouseholdIDSearch.ToString()))
            {
                members = members.Where(p => p.HouseholdID == HouseholdIDSearch);
                ViewData["Filtering"] = "btn-danger";
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
                else
                {
                    sortDirection = string.IsNullOrEmpty(sortDirectionCheck) ? "asc" : "desc";
                    sortField = sortFieldID;
                }
            }

            //Now we know which field and direction to sort by
            if (sortField == "Member")
                    {
                        if (sortDirection == "asc")
                        {
                            members = members
                        .OrderBy(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);

                        }
                        else
                        {
                            members = members
                               .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                        }
                    }
                    else if (sortField == "Age")
                    {
                        if (sortDirection == "asc")
                        {
                            members = members
                                .OrderByDescending(p => p.DOB)
                        .ThenByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                        }
                        else
                        {
                            members = members
                                 .OrderBy(p => p.DOB)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                        }
                    }
                    else if (sortField == "Gender")
                    {
                        if (sortDirection == "asc")
                        {
                            members = members
                                 .OrderBy(p => p.Gender.Name)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                        }
                        else
                        {
                            members = members
                                .OrderByDescending(p => p.Gender.Name)
                        .ThenByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                        }
                            }
                    /*else if (sortField == "Situation")
                    {
                        if (sortDirection == "asc")
                        {
                            members = members
                                 .OrderBy(p => p.IncomeSituation.Situation)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                        }
                        else
                        {
                            members = members
                                .OrderByDescending(p => p.IncomeSituation.Situation)
                        .ThenByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                        }
                    }*/



            //Set sort for next time
            ViewData["sortField"] = sortField;
            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = ""; //Asume not filtering
            ViewData["sortDirection"] = sortDirection;
            //selectlist for Sorting options
            ViewData["Action"] = "/Orders";
            ViewData["Modals"] = new List<string> { "_PageSizeModal", "_CreateOrderModal" };

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Member>.CreateAsync(members.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Orders).ThenInclude(o => o.PaymentType)
                .Include(m => m.Gender)
                .Include(m => m.Household).ThenInclude(h => h.City)
                .Include(m => m.Household).ThenInclude(h => h.Province)
                .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
                .Include(m => m.MemberDocuments)
                .Include(m => m.DietaryRestrictionMembers).ThenInclude(drm => drm.DietaryRestriction)
                .FirstOrDefaultAsync(m => m.ID == id);


            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {

            var member = new Member();

            PopulateDropDownLists(member);
            return View();

            /* ViewData["GenderID"] = new SelectList(_context.Genders, "ID", "Name");
             ViewData["HouseholdID"] = new SelectList(_context.Households, "ID", "City");
             ViewData["IncomeSituationID"] = new SelectList(_context.IncomeSituations, "ID", "Situation");
             return View();*/
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,DOB,PhoneNumber,Email,Notes,YearlyIncome,GenderID,HouseholdID,IncomeSituationID")] Member member)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(member);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {

                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
         
            PopulateDropDownLists(member);
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Household).ThenInclude(h => h.City)
                .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
                .Include(m => m.Gender)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (member == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(member);
            return View(member);

        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,FirstName,MiddleName,LastName,DOB,PhoneNumber,Email,Notes,YearlyIncome,GenderID,HouseholdID,IncomeSituationID")] Member member)
        {
            //get member to update
            var memberToUpdate = await _context.Members
              .Include(h => h.Household)
              .Include(h => h.Gender)
              .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
              .SingleOrDefaultAsync(h => h.ID == id);

            //Check that you got it or exit with a not found error
            if (memberToUpdate == null)
            {
                return NotFound();
            }


            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
                m => m.FirstName, m => m.LastName, m => m.MiddleName, m => m.DOB, m => m.GenderID, m => m.Email,
                m => m.Notes, m => m.HouseholdID))
            {
                try
                {

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Members", new { ID = memberToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(memberToUpdate.ID))
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


            PopulateDropDownLists(memberToUpdate);
            return View(memberToUpdate);

        }

        // GET: Members/Delete/5
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Gender)
                .Include(m => m.Household).ThenInclude(h => h.City)
                .Include(m => m.Household).ThenInclude(h => h.Province)
                .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.ID == id);
        }

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.FileContent.MimeType, theFile.FileName);
        }
        private SelectList HouseholdSelectList(int? selectedId)
        {
            return new SelectList(_context.Households
                .OrderBy(d => d.ID), "ID", "ID", selectedId);
                
        }
        private SelectList GenderSelectList(int? selectedId)
        {
            return new SelectList(_context.Genders
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private SelectList IncomeSelectList(int? selectedId)
        {
            return new SelectList(_context.IncomeSituations
                .OrderBy(d => d.Situation), "ID", "Situation", selectedId);
        }

        public PartialViewResult MemberIncomeSituationList(int id)
        {
            ViewBag.MemberIncomeSituations = _context.MemberIncomeSituations
                .Include(s => s.IncomeSituation)
                .Where(s => s.MemberID == id)
                .OrderBy(s => s.IncomeSituation.Situation)
                .ToList();
            return PartialView("_MemberIncomeSituationList");
        }

        private void PopulateDropDownLists(Member member = null)
        {
            ViewData["HouseholdID"] = HouseholdSelectList(member?.HouseholdID);
            ViewData["GenderID"] = GenderSelectList(member?.GenderID);
            ViewData["IncomeSituationID"] = IncomeSelectList(null);
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
