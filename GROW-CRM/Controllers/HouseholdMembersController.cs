using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using GROW_CRM.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace GROW_CRM.Controllers
{
    public class HouseholdMembersController : Controller
    {
        private readonly GROWContext _context;

        public HouseholdMembersController(GROWContext context)
        {
            _context = context;
        }

        // GET: PatientAppt
        public async Task<IActionResult> Index(int? HouseholdID, int? page, int? pageSizeID, int? IncomeSituationID, string actionButton,
            string NotesSearchString, string MemberSearchString, string sortDirection = "desc", string sortField = "Member")
        {
            //Get the URL with the last filter, sort and page parameters from THE PATIENTS Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Households");

            if (!HouseholdID.HasValue)
            {
                //Get the proper return URL for the Patients controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            PopulateDropDownLists();

            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = "btn-outline-dark"; //Asume not filtering
            //Then in each "test" for filtering, add ViewData["Filtering"] = "btn-danger" if true;

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Income Situation", "Member", "Age" };

            var members = from m in _context.Members
                                  .Include(m => m.Gender)
                                  .Include(m => m.Household)
                                  .Include(m => m.IncomeSituation)
                        where m.HouseholdID == HouseholdID.GetValueOrDefault()
                        select m;

            if (IncomeSituationID.HasValue)
            {
                members = members.Where(p => p.IncomeSituationID == IncomeSituationID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(NotesSearchString))
            {
                members = members.Where(p => p.Notes.ToUpper().Contains(NotesSearchString.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(MemberSearchString))
            {
                members = members.Where(p => p.LastName.ToUpper().Contains(MemberSearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(MemberSearchString.ToUpper()));

                //(p => p.FullName.ToUpper().Contains(MemberSearchString.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset back to first page when sorting or filtering

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by.
            if (sortField == "Income Situation")
            {
                if (sortDirection == "asc")
                {
                    members = members
                        .OrderBy(m => m.IncomeSituation.Situation);
                }
                else
                {
                    members = members
                        .OrderByDescending(m => m.IncomeSituation.Situation);
                }
            }
            else if (sortField == "Member")
            {
                if (sortDirection == "asc")
                {
                    members = members
                        .OrderBy(m => m.LastName)
                        .ThenBy(m => m.FirstName);
                }
                else
                {
                    members = members
                        .OrderByDescending(m => m.LastName)
                        .ThenByDescending(m => m.FirstName);
                }
            }
            else //Age
            {
                if (sortDirection == "asc")
                {
                    members = members
                        .OrderBy(m => m.DOB);
                }
                else
                {
                    members = members
                        .OrderByDescending(m => m.DOB);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Now get the MASTER record, the patient, so it can be displayed at the top of the screen
            Household household = _context.Households
                .Include(h => h.City)
                .Include(h => h.Province)
                .Include(h => h.HouseholdStatus)
                .Include(h => h.Members)
                .Where(h => h.ID == HouseholdID.GetValueOrDefault()).FirstOrDefault();
            ViewBag.Household = household;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Member>.CreateAsync(members.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }


        // GET: PatientAppt/Add
        public IActionResult Add(int? HouseholdID, string HouseholdAddress)
        {
            if (!HouseholdID.HasValue)
            {
                return RedirectToAction("Index", "Patients");
            }

            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            ViewData["HouseholdAddress"] = HouseholdAddress;
            Member m = new Member()
            {
                HouseholdID = HouseholdID.GetValueOrDefault()
            };

            PopulateAssignedDietaryRestrictionData(m);
            PopulateDropDownLists();
            return View(m);
        }

        // POST: PatientAppt/Add
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,FirstName,MiddleName,LastName,DOB,PhoneNumber,Email,Notes,YearlyIncome,GenderID,HouseholdID,IncomeSituationID")] Member member, string HouseholdAddress, string[] selectedOptions)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            try
            {
                //Add the selected conditions
                if (selectedOptions != null)
                {
                    foreach (var restriction in selectedOptions)
                    {
                        var restrictionToAdd = new DietaryRestrictionMember { MemberID = member.ID, DietaryRestrictionID = int.Parse(restriction) };
                        member.DietaryRestrictionMembers.Add(restrictionToAdd);
                    }
                }
                if (ModelState.IsValid)
                {
                    _context.Add(member);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
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

            PopulateAssignedDietaryRestrictionData(member);
            PopulateDropDownLists(member);
            ViewData["HouseholdAddress"] = HouseholdAddress;
            return View(member);
        }

        // GET: PatientAppt/Update/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            var member = await _context.Members
               .Include(m => m.Gender)
               .Include(m => m.Household)
               .Include(m => m.IncomeSituation)
               .Include(m => m.DietaryRestrictionMembers).ThenInclude(drm => drm.DietaryRestriction)
               .FirstOrDefaultAsync(m => m.ID == id);

            if (member == null)
            {
                return NotFound();
            }

            PopulateAssignedDietaryRestrictionData(member);
            PopulateDropDownLists(member);
            return View(member);
        }

        // POST: PatientAppt/Update/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string[] selectedOptions)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            var memberToUpdate = await _context.Members
                .Include(m => m.Gender)
                //.Include(m => m.Household)
                .Include(m => m.IncomeSituation)
                .Include(m => m.DietaryRestrictionMembers).ThenInclude(drm => drm.DietaryRestriction)
                .FirstOrDefaultAsync(m => m.ID == id);



            //Check that you got it or exit with a not found error
            if (memberToUpdate == null)
            {
                return NotFound();
            }

            //Update Dietary Restrictions
            UpdateDietaryRestrictionMembers(selectedOptions, memberToUpdate);

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
                m => m.FirstName, m => m.MiddleName, m => m.LastName, p => p.DOB, m => m.PhoneNumber,
                m => m.Email, m => m.Notes, m => m.YearlyIncome, m => m.GenderID, m => m.IncomeSituationID))
            {
                try
                {
                    _context.Update(memberToUpdate);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
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
                    Console.WriteLine("Something Went Wrong");
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateAssignedDietaryRestrictionData(memberToUpdate);
            PopulateDropDownLists(memberToUpdate);
            return View(memberToUpdate);
        }

        // GET: PatientAppt/Remove/5
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            var member = await _context.Members
                .Include(m => m.Gender)
                .Include(m => m.Household).ThenInclude(h => h.City)
                .Include(m => m.Household).ThenInclude(h => h.Province)
                .Include(m => m.IncomeSituation)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: PatientAppt/Remove/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var member = await _context.Members
                .Include(m => m.Gender)
                .Include(m => m.Household)
                .Include(m => m.IncomeSituation)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            try
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(member);
        }

        private SelectList GenderSelectList(int? id)
        {
            var gQuery = from g in _context.Genders
                         orderby g.Name
                         select g;
            return new SelectList(gQuery, "ID", "Name", id);
        }

        private SelectList IncomeSituationSelectList(int? id)
        {
            var incQuery = from inc in _context.IncomeSituations
                           orderby inc.Situation
                           select inc;
            return new SelectList(incQuery, "ID", "Situation", id);
        }

        private void PopulateDropDownLists(Member member = null)
        {
            ViewData["GenderID"] = GenderSelectList(member?.GenderID);
            ViewData["IncomeSituationID"] = IncomeSituationSelectList(member?.IncomeSituationID);
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }
        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private void PopulateAssignedDietaryRestrictionData(Member member)
        {
            //For this to work, you must have Included the PatientConditions 
            //in the Patient
            var allOptions = _context.DietaryRestrictions;
            var currentOptionIDs = new HashSet<int>(member.DietaryRestrictionMembers.Select(b => b.DietaryRestrictionID));
            var checkBoxes = new List<OptionVM>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new OptionVM
                {
                    ID = option.ID,
                    DisplayText = option.Restriction,
                    Assigned = currentOptionIDs.Contains(option.ID)
                });
            }
            ViewData["RestrictionOptions"] = checkBoxes;
        }
        private void UpdateDietaryRestrictionMembers(string[] selectedOptions, Member memberToUpdate)
        {
            if (selectedOptions == null)
            {
                memberToUpdate.DietaryRestrictionMembers = new List<DietaryRestrictionMember>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var memberOptionsHS = new HashSet<int>
                (memberToUpdate.DietaryRestrictionMembers.Select(c => c.DietaryRestrictionID));//IDs of the currently selected conditions
            foreach (var option in _context.DietaryRestrictions)
            {
                if (selectedOptionsHS.Contains(option.ID.ToString())) //It is checked
                {
                    if (!memberOptionsHS.Contains(option.ID))  //but not currently in the history
                    {
                        memberToUpdate.DietaryRestrictionMembers.Add(new DietaryRestrictionMember { MemberID = memberToUpdate.ID, DietaryRestrictionID = option.ID });
                    }
                }
                else
                {
                    //Checkbox Not checked
                    if (memberOptionsHS.Contains(option.ID)) //but it is currently in the history - so remove it
                    {
                        DietaryRestrictionMember dietaryRestrictionToRemove = memberToUpdate.DietaryRestrictionMembers.SingleOrDefault(c => c.DietaryRestrictionID == option.ID);
                        _context.Remove(dietaryRestrictionToRemove);
                    }
                }
            }
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.ID == id);
        }
    }
}
