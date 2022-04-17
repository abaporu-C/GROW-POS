using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using GROW_CRM.ViewModels;
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
    public class NotificationTypesController : Controller
    {

        //for sending email
        private readonly IMyEmailSender _emailSender;
        private readonly GROWContext _context;

        public NotificationTypesController(GROWContext context, IMyEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: NotificationTypes
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
        }

        // GET: NotificationTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (notificationType == null)
            {
                return NotFound();
            }

            return View(notificationType);
        }

        // GET: NotificationTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NotificationTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Type")] NotificationType notificationType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notificationType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
            }
            return View(notificationType);
        }

        // GET: NotificationTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes.FindAsync(id);
            if (notificationType == null)
            {
                return NotFound();
            }
            return View(notificationType);
        }

        // POST: NotificationTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Type")] NotificationType notificationType)
        {
            if (id != notificationType.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notificationType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationTypeExists(notificationType.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
            }
            return View(notificationType);
        }

        // GET: NotificationTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationType = await _context.NotificationTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (notificationType == null)
            {
                return NotFound();
            }

            return View(notificationType);
        }

        // POST: NotificationTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notificationType = await _context.NotificationTypes.FindAsync(id);
            _context.NotificationTypes.Remove(notificationType);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Lookups", new { Tab = ControllerName() + "Tab" });
        }

        // GET/POST: MembersbyStatus/Notification/5
        public async Task<IActionResult> Notification(int? id, string Subject, string emailContent, int? householdStatusID)
        {
            PopulateDropDownLists();
            //add a variable that calls HouseholdStatus ID...but how?
            var HouseholdStatusID = await _context.Members
                .Include(m => m.Household)
                .ThenInclude(m=>m.HouseholdStatus)
                .FirstOrDefaultAsync(m => m.Household.HouseholdStatusID == householdStatusID);
            

            if (id == null)
            {
                return NotFound();
            }
            NotificationType t = await _context.NotificationTypes.FindAsync(id);

            ViewData["householdStatusID"] = householdStatusID;
            ViewData["id"] = id;
            ViewData["Type"] = t.Type;

            if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
            {
                ViewData["Message"] = "You must enter both a Subject and some message Content before sending the message.";
            }
            else
            {
                int folksCount = 0;
                try
                {
                    //Send a Notice.
                    List<EmailAddress> folks = (from p in _context.Members
                                                where p.Household.HouseholdStatusID == householdStatusID
                                                select new EmailAddress
                                                {
                                                    Name = p.FullName,
                                                    Address = p.Email
                                                }).ToList();
                    folksCount = folks.Count();
                    if (folksCount > 0)
                    {
                        foreach(EmailAddress email in folks)
                        {
                            var msg = new EmailMessage()
                            {
                                ToAddresses = new List<EmailAddress> { email },
                                Subject = Subject,
                                Content = "<p>" + emailContent + "</p><p>Please access the <strong>GROW Food Literacy Centre</strong> web site to review.</p>"

                            };
                            await _emailSender.SendToManyAsync(msg);
                            ViewData["Message"] = "Message sent to " + folksCount + " Member"
                                + ((folksCount == 1) ? "." : "s.");
                        }                        
                    }
                    else
                    {
                        ViewData["Message"] = "Message NOT sent!  No Members in status selected.";
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = ex.GetBaseException().Message;
                    ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Member"
                        + ((folksCount == 1) ? "" : "s") + " in the List.";
                }
            }
            PopulateDropDownLists();
            return View();
        }

        private string ControllerName()
        {
            return this.ControllerContext.RouteData.Values["controller"].ToString();
        }

        private bool NotificationTypeExists(int id)
        {
            return _context.NotificationTypes.Any(e => e.ID == id);
        }

        private SelectList HouseholdStatusSelectList(int? selectedId)
        {
            SelectList sl = new SelectList(_context.HouseholdStatuses
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
            return sl;

        }

        private void PopulateDropDownLists(Household household = null)
        {
            ViewBag.householdStatusID = HouseholdStatusSelectList(household?.HouseholdStatusID);
        }
    }
}
