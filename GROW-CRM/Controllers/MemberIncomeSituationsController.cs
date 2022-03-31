using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize]
    public class MemberIncomeSituationsController : Controller
    {
        private readonly GROWContext _context;
        public MemberIncomeSituationsController(GROWContext context)
        {
            _context = context;
        }

        // GET: Member's Income Situations
        public IActionResult Index()
        {
            return RedirectToAction("Index", "HouseholdMembers");
        }

        public PartialViewResult CreateMemberIncomeSituation(int? ID)
        {
            //For this action, the ID parameter is the ID of the Athlete

            //This is a classic NOT IN query done in LINQ
            //So we don't offer options already taken
            var unusedSponsors = from mis in _context.IncomeSituations
                                 where !(from p in _context.MemberIncomeSituations
                                         where p.MemberID == ID
                                         select p.IncomeSituationID).Contains(mis.ID)
                                 select mis;

            ViewBag.IncomeSituationID = new
                SelectList(unusedSponsors
                .OrderBy(a => a.Situation), "ID", "Situation");

            //So we can save it in tne Form
            ViewData["MemberID"] = ID.GetValueOrDefault();

            return PartialView("_CreateMemberIncomeSituation");
        }

        /*[HttpPost]
        public PartialViewResult CreateFakerMemberIncomeSituation()
        {
            //For this action, the ID parameter is the ID of the Athlete

            //This is a classic NOT IN query done in LINQ
            //So we don't offer options already taken
            var unusedSponsors = from mis in _context.IncomeSituations
                                 select mis;

            ViewBag.IncomeSituationID = new
                SelectList(unusedSponsors
                .OrderBy(a => a.Situation), "ID", "Situation");

            return PartialView("_CreateFakerMemberIncomeSituation");
        }*/

        public PartialViewResult EditMemberIncomeSituation(int ID)
        {
            //Get the Sponsorship to edit
            var memberIS = _context.MemberIncomeSituations.Find(ID);

            //Use it to help filter the SelectList so you do not offer options
            //that are already taken.
            //This is a classic NOT IN query done in LINQ but also include the current selection
            var unusedSponsors = from sp in _context.IncomeSituations
                                 where !(from p in _context.MemberIncomeSituations
                                         where p.MemberID == memberIS.MemberID
                                         select p.IncomeSituationID).Contains(sp.ID)
                                    || sp.ID == memberIS.IncomeSituationID //Add in the current sponsor
                                 select sp;

            ViewData["IncomeSituationID"] = new
                SelectList(unusedSponsors
                .OrderBy(a => a.Situation), "ID", "Situation", memberIS.IncomeSituationID);

            return PartialView("_EditMemberIncomeSituation", memberIS);
        }

        public PartialViewResult DeleteMemberIncomeSituation(int Id)
        {
            //Get the one to delete
            MemberIncomeSituation sponsorship = _context.MemberIncomeSituations
                .Include(p => p.IncomeSituation)
                .Where(p => p.ID == Id)
                .FirstOrDefault();

            return PartialView("_DeleteMemberIncomeSituation", sponsorship);
        }

        // POST: Sponsorships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberID,IncomeSituationID,Income")] MemberIncomeSituation incomeSource)
        {
            try
            {
                var member = _context.Members.Where(m => m.ID == incomeSource.MemberID).FirstOrDefault();

                if(member == null)
                {
                    throw new ForeignKeyException();
                }

                if (ModelState.IsValid)
                {
                    _context.Add(incomeSource);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (ForeignKeyException)
            {
                ModelState.AddModelError("", "There was a problem during the creation of this member. Please, start the process again.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }            

            return View(incomeSource);
        }

        // POST: Sponsorships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int ID)
        {
            MemberIncomeSituation sponsorshipToUpdate = await _context.MemberIncomeSituations.FindAsync(ID);
            if (await TryUpdateModelAsync<MemberIncomeSituation>(sponsorshipToUpdate, "",
                p => p.IncomeSituationID, p => p.Income))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberIncomeSituationExists(sponsorshipToUpdate.ID))
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
            return View(sponsorshipToUpdate);
        }

        // POST: Sponsorships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int ID)
        {
            var sponsorship = await _context.MemberIncomeSituations.FindAsync(ID);
            try
            {
                _context.MemberIncomeSituations.Remove(sponsorship);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(sponsorship);
        }

        private bool MemberIncomeSituationExists(int id)
        {
            return _context.MemberIncomeSituations.Any(e => e.ID == id);
        }
    }
}
