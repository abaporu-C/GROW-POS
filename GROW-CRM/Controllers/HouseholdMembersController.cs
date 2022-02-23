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
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GROW_CRM.Controllers
{
    public class HouseholdMembersController : Controller
    {
        private readonly GROWContext _context;

        public HouseholdMembersController(GROWContext context)
        {
            _context = context;
        }

        // GET: Household Members
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
                                  .Include(m => m.MemberDocuments)
                                  .Include(m => m.MemberIncomeSituations)
                        where m.HouseholdID == HouseholdID.GetValueOrDefault()
                        select m;

            List<List<MemberIncomeSituation>> misList = new List<List<MemberIncomeSituation>>();

            foreach(Member m in members)
            {
                var v = _context.MemberIncomeSituations
                .Include(s => s.IncomeSituation)
                .Where(s => s.MemberID == m.ID)
                .OrderBy(s => s.IncomeSituation.Situation)
                .ToList();

                misList.Add(v);
            }

            ViewBag.MisList = misList;

            /*if (IncomeSituationID.HasValue)
            {
                members = members.Where(p => p.IncomeSituationID == IncomeSituationID);
                ViewData["Filtering"] = "btn-danger";
            }*/
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
            /*if (sortField == "Income Situation")
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
            }*/
            if (sortField == "Member")
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
        public async Task<IActionResult> Add(int? HouseholdID, string HouseholdName)
        {
            if (!HouseholdID.HasValue)
            {
                return RedirectToAction("Index", "Members");
            }

         

            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            ViewData["HouseholdName"] = HouseholdName;
            ViewData["MISList"] = new List<MemberIncomeSituationVM>();
            Member m = new Member()
            {
                FirstName = "",
                LastName = "",
                DOB = DateTime.Now,
                PhoneNumber = "",
                Email = "",
                GenderID = 1,
                HouseholdID = HouseholdID.GetValueOrDefault()
            };

            _context.Add(m);
            await _context.SaveChangesAsync();            

            PopulateAssignedDietaryRestrictionData(m);
            PopulateDropDownLists();
            return View(m);
        }

        // POST: PatientAppt/Add
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,Name,FirstName,MiddleName,LastName,DOB,PhoneNumber,Email,Notes,ConsentGiven,GenderID,HouseholdID,IncomeSituationID")] Member member, string HouseholdName, string[] selectedIllnessOptions, string[] selectedConcernOptions, List<IFormFile> theFiles)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            try
            {
                //Add the selected conditions
                if (selectedIllnessOptions != null)
                {
                    foreach (var restriction in selectedIllnessOptions)
                    {
                        var restrictionToAdd = new DietaryRestrictionMember { MemberID = member.ID, DietaryRestrictionID = int.Parse(restriction) };
                        member.DietaryRestrictionMembers.Add(restrictionToAdd);
                    }
                }
                if (selectedConcernOptions != null)
                {
                    foreach (var restriction in selectedConcernOptions)
                    {
                        var restrictionToAdd = new DietaryRestrictionMember { MemberID = member.ID, DietaryRestrictionID = int.Parse(restriction) };
                        member.DietaryRestrictionMembers.Add(restrictionToAdd);
                    }
                }

                var memberToUpdate = await _context.Members
                .Include(m => m.Gender)
                .Include(m => m.Household)
                .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
                .Include(m => m.MemberDocuments).ThenInclude(m => m.DocumentType)
                .Include(m => m.DietaryRestrictionMembers).ThenInclude(drm => drm.DietaryRestriction)
                .FirstOrDefaultAsync(m => m.ID == member.ID);

                if (ModelState.IsValid && await TryUpdateModelAsync<Member>(memberToUpdate, "",
                m => m.FirstName, m => m.MiddleName, m => m.LastName, p => p.DOB, m => m.PhoneNumber,
                m => m.Email, m => m.Notes, m => m.ConsentGiven, m => m.GenderID))
                {
                    _context.Update(memberToUpdate);
                    await CheckLICO(memberToUpdate);
                    await AddDocumentsAsync(memberToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    ViewData["returnURL"] = $"/HouseholdMembers?HouseholdID={member.HouseholdID}";
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException ex)
            {
                string msg = ex.Message;
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            PopulateAssignedDietaryRestrictionData(member);
            PopulateDropDownLists(member);
            ViewData["HouseholdName"] = HouseholdName;
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
               .Include(m => m.MemberDocuments).ThenInclude(m => m.DocumentType)
               .Include(m => m.MemberIncomeSituations)
               .Include(m => m.DietaryRestrictionMembers).ThenInclude(drm => drm.DietaryRestriction)
               .FirstOrDefaultAsync(m => m.ID == id);

            List<MemberIncomeSituation> misList = _context.MemberIncomeSituations
                                                            .Include(s => s.IncomeSituation)
                                                            .Where(s => s.MemberID == member.ID)
                                                            .OrderBy(s => s.IncomeSituation.Situation)
                                                            .ToList();            

            ViewBag.MisList = misList;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string[] selectedIllnessOptions, string[] selectedConcernOptions, List<IFormFile> theFiles)
        {
            //Get the URL with the last filter, sort and page parameters
            ViewDataReturnURL();

            var memberToUpdate = await _context.Members
                .Include(m => m.Gender)
                .Include(m => m.Household)
                .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
                .Include(m => m.MemberDocuments).ThenInclude(m => m.DocumentType)
                .Include(m => m.DietaryRestrictionMembers).ThenInclude(drm => drm.DietaryRestriction)
                .FirstOrDefaultAsync(m => m.ID == id);



            //Check that you got it or exit with a not found error
            if (memberToUpdate == null)
            {
                return NotFound();
            }

            //Update Dietary Restrictions
            UpdateDietaryRestrictionMembers(selectedIllnessOptions, selectedConcernOptions,memberToUpdate);

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Member>(memberToUpdate, "",
                m => m.FirstName, m => m.MiddleName, m => m.LastName, p => p.DOB, m => m.PhoneNumber,
                m => m.Email, m => m.Notes, m => m.ConsentGiven, m => m.GenderID))
            {
                try
                {                    
                    _context.Update(memberToUpdate);
                    await CheckLICO(memberToUpdate);
                    await AddDocumentsAsync(memberToUpdate, theFiles);
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
                .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
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
                .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
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

        private SelectList DocumentTypeSelectList()
        {
            var dtQuery = from dt in _context.DocumentTypes
                          orderby dt.Type
                          select dt;

            return new SelectList(dtQuery, "ID", "Type");
        }

        private SelectList IncomeSituationSelectList(int? id)
        {
            var incQuery = from inc in _context.IncomeSituations
                           orderby inc.Situation
                           select inc;
            return new SelectList(incQuery, "ID", "Situation", id);
        }

        [HttpGet]
        public JsonResult GetIncomeSituations(int? id)
        {
            return Json(IncomeSituationSelectList(id));
        }

        private void PopulateDropDownLists(Member member = null)
        {
            ViewData["GenderID"] = GenderSelectList(member?.GenderID);
            ViewData["DocumentTypeID"] = DocumentTypeSelectList();
            ViewData["IncomeSituationID"] = IncomeSituationSelectList(null);
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
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.DietaryRestrictions.Include(dr => dr.HealthIssueType);
            var currentOptionsHS = new HashSet<int>(member.DietaryRestrictionMembers.Select(b => b.DietaryRestrictionID));
            //Instead of one list with a boolean, we will make two lists
            var selectedIllnesses = new List<ListOptionVM>();
            var availableIllnesses = new List<ListOptionVM>();
            var selectedConcerns = new List<ListOptionVM>();
            var availableConcerns = new List<ListOptionVM>();

            foreach (var option in allOptions)
            {
                if (currentOptionsHS.Contains(option.ID))
                {
                    if(option.HealthIssueType.Type == "Illness") selectedIllnesses.Add(new ListOptionVM{ID = option.ID,DisplayText = option.Restriction});
                    if (option.HealthIssueType.Type == "Concern") selectedConcerns.Add(new ListOptionVM { ID = option.ID, DisplayText = option.Restriction });
                }
                else
                {
                    if (option.HealthIssueType.Type == "Illness") availableIllnesses.Add(new ListOptionVM { ID = option.ID, DisplayText = option.Restriction });
                    if (option.HealthIssueType.Type == "Concern") availableConcerns.Add(new ListOptionVM { ID = option.ID, DisplayText = option.Restriction });
                }
            }
            ViewData["selIllnessOpts"] = new MultiSelectList(selectedIllnesses.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availIllnessOpts"] = new MultiSelectList(availableIllnesses.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["selConcernOpts"] = new MultiSelectList(selectedConcerns.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availConcernOpts"] = new MultiSelectList(availableConcerns.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateDietaryRestrictionMembers(string[] selectedIllnessOptions, string[] selectedConcernOptions, Member memberToUpdate)
        {
            if (selectedIllnessOptions == null && selectedConcernOptions == null)
            {
                memberToUpdate.DietaryRestrictionMembers = new List<DietaryRestrictionMember>();
                return;
            }

            var selectedOptionsHS = new List<string>();

            if (selectedIllnessOptions != null)
                selectedOptionsHS.AddRange(selectedIllnessOptions);

            if (selectedConcernOptions != null)
                selectedOptionsHS.AddRange(selectedConcernOptions);

            var memberOptionsHS = new HashSet<int>(memberToUpdate.DietaryRestrictionMembers.Select(c => c.DietaryRestrictionID));//IDs of the currently selected conditions
            
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

        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.FileContent.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Member member, List<IFormFile> theFiles)
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
                        member.MemberDocuments.Add(d);
                    };
                }
            }
        }

        private async Task CheckLICO(Member m)
        {
            var household = await _context.Households
                            .Include(h => h.Members).ThenInclude(m => m.MemberIncomeSituations)
                            .Where(h => h.ID == m.HouseholdID)
                            .FirstOrDefaultAsync();

            double totalIncome = m.YearlyIncome;
            int memberCount = household.Members.Count();

            foreach(Member member in household.Members)
            {
                totalIncome += member.YearlyIncome;
            }

            if (totalIncome == 0) household.LICOVerified = true;
            else if (memberCount == 1 && totalIncome > 26426) household.LICOVerified = false;
            else if (memberCount == 2 && totalIncome > 32898) household.LICOVerified = false;
            else if (memberCount == 3 && totalIncome > 40444) household.LICOVerified = false;
            else if (memberCount == 4 && totalIncome > 49106) household.LICOVerified = false;
            else if (memberCount == 5 && totalIncome > 55694) household.LICOVerified = false;
            else if (memberCount == 6 && totalIncome > 62814) household.LICOVerified = false;
            else if (memberCount == 7 && totalIncome > 69934) household.LICOVerified = false;
            else
            {
                if (memberCount > 7)
                {
                    double lico = 69934;
                    for (int i = 0; i < (memberCount - 7); i++) lico += 7120;

                    if (totalIncome > lico) household.LICOVerified = false;
                    else household.LICOVerified = true;
                }
                else household.LICOVerified = true;
            }

            _context.Update(household);

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

        public async Task<IActionResult> CancelMember(string direction, int MemberID, int? HouseholdID)
        {
            var member = await _context.Members
                .Include(m => m.Gender)
                .Include(m => m.Household)
                .Include(m => m.MemberIncomeSituations).ThenInclude(mis => mis.IncomeSituation)
                .FirstOrDefaultAsync(m => m.ID == MemberID);

            try
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();

                switch (direction)
                {
                    case "households":
                        return RedirectToAction("Index", "Households");
                    case "householdDetails":
                        ViewData["returnURL"] = $"/HouseholdMembers?HouseholdID={HouseholdID}";
                        return Redirect(ViewData["returnURL"].ToString());
                    default:
                        break;
                }

                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            ViewData["returnURL"] = $"/HouseholdMembers?HouseholdID={HouseholdID}";
            return Redirect(ViewData["returnURL"].ToString());
        }
        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.ID == id);
        }
    }
}
