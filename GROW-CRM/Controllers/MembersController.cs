using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;

namespace GROW_CRM.Controllers
{
    public class MembersController : Controller
    {
        private readonly GROWContext _context;

        public MembersController(GROWContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index(string MemberSearch, string PhoneSearch, string HouseholdSearch, 
            string HouseholdCodeSearch,
            int? HouseholdID, int? GenderID, 
            int? page, int? pageSizeID, string actionButton,
            string sortDirection = "asc", string sortField = "Member")
        {
            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = ""; //Asume not filtering

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Member", "Age", "Gender", "Household" };

            PopulateDropDownLists();

            var members = from m in _context.Members
                              .Include(m => m.Gender)
                              .Include(m => m.Household).ThenInclude(h => h.City)
                              .Include(m => m.IncomeSituation)
                              select m;


            //Add as many filters as needed
            if (HouseholdID.HasValue)
            {
                members = members.Where(p => p.HouseholdID == HouseholdID);
                ViewData["Filtering"] = " show";
            }
            if (GenderID.HasValue)
            {
                members = members.Where(p => p.GenderID == GenderID);
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(MemberSearch))
            {
                members = members.Where(p => p.LastName.ToUpper().Contains(MemberSearch.ToUpper())
                                       || p.FirstName.ToUpper().Contains(MemberSearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(PhoneSearch))
            {
                members = members.Where(p => p.PhoneNumber.Contains(PhoneSearch));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(HouseholdSearch))
            {
                members = members.Where(p => p.Household.StreetName.ToUpper().Contains(HouseholdSearch.ToUpper())
                                       || p.Household.City.Name.ToUpper().Contains(HouseholdSearch.ToUpper()));
                ViewData["Filtering"] = " show";
            }
            if (!String.IsNullOrEmpty(HouseholdCodeSearch))
            {
                members = members.Where(p => p.Household.HouseholdCode.Contains(HouseholdCodeSearch));
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
                   /* else //Sorting by Athlete Name
                    {
                        if (sortDirection == "asc")
                        {
                            members = members
                                .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                        }
                        else
                        {
                            members = members
                                .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                        }
                    }*/
                
            

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            return View(await members.ToListAsync());
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Gender)
                .Include(m => m.Household)
                .Include(m => m.IncomeSituation)
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
            ViewData["GenderID"] = new SelectList(_context.Genders, "ID", "Name");
            ViewData["HouseholdID"] = new SelectList(_context.Households, "ID", "City");
            ViewData["IncomeSituationID"] = new SelectList(_context.IncomeSituations, "ID", "Situation");
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,DOB,PhoneNumber,Email,Notes,GenderID,HouseholdID,IncomeSituationID")] Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenderID"] = new SelectList(_context.Genders, "ID", "ID", member.GenderID);
            ViewData["HouseholdID"] = new SelectList(_context.Households, "ID", "ID", member.HouseholdID);
            ViewData["IncomeSituationID"] = new SelectList(_context.IncomeSituations, "ID", "Situation", member.IncomeSituationID);
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            ViewData["GenderID"] = new SelectList(_context.Genders, "ID", "ID", member.GenderID);
            ViewData["HouseholdID"] = new SelectList(_context.Households, "ID", "City", member.HouseholdID);
            ViewData["IncomeSituationID"] = new SelectList(_context.IncomeSituations, "ID", "ID", member.IncomeSituationID);
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,FirstName,MiddleName,LastName,DOB,PhoneNumber,Email,Notes,GenderID,HouseholdID,IncomeSituationID")] Member member)
        {
            if (id != member.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.ID))
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
            ViewData["GenderID"] = new SelectList(_context.Genders, "ID", "ID", member.GenderID);
            ViewData["HouseholdID"] = new SelectList(_context.Households, "ID", "City", member.HouseholdID);
            ViewData["IncomeSituationID"] = new SelectList(_context.IncomeSituations, "ID", "ID", member.IncomeSituationID);
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.Gender)
                .Include(m => m.Household)
                .Include(m => m.IncomeSituation)
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

        private SelectList HouseholdSelectList(int? selectedId)
        {
            return new SelectList(_context.Households
                .OrderBy(d => d.City), "ID", "Name", selectedId);
                
        }
        private SelectList GenderSelectList(int? selectedId)
        {
            return new SelectList(_context.Genders
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(Member member = null)
        {
            ViewData["HouseholdID"] = HouseholdSelectList(member?.HouseholdID);
           
            ViewData["GenderID"] = GenderSelectList(member?.GenderID);
           
        }
    }
}
