using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using GROW_CRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebPush;

namespace GROW_CRM.Controllers
{
    [Authorize]
    public class EmployeeAccountsController : Controller
    {
        //Specialized controller just used to allow an 
        //Authenticated user to maintain their own  account details.

        private readonly GROWContext _context;
        private readonly IConfiguration _configuration;

        public EmployeeAccountsController(GROWContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: EmployeeAccount
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Details));
        }

        // GET: EmployeeAccount/Details/5
        public async Task<IActionResult> Details()
        {
            //Check if we are configured for Push
            if (String.IsNullOrEmpty(_configuration.GetSection("VapidKeys")["PublicKey"]))
            {
                return RedirectToAction("GenerateKeys");
            }

            var employee = await _context.Employees
               .Include(e => e.Subscriptions)
               .Where(c => c.Email == User.Identity.Name)
               .Select(c=> new EmployeeVM
               {
                   ID = c.ID,
                   FirstName = c.FirstName,
                   LastName = c.LastName,
                   Phone = c.Phone,
                   Email = c.Email,
                   Active = c.Active,
                   NumberOfPushSubscriptions = c.Subscriptions.Count()
               })
               .FirstOrDefaultAsync();
            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.PublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];

            return View(employee);
        }

        // GET: EmployeeAccount/ Create the record of the Push Subscription
        public IActionResult Push(int EmployeeID)
        {
            ViewBag.PublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            ViewData["EmployeeID"] = EmployeeID;
            return View();
        }

        // POST: EmployeeAccount/ Create the record of the Push Subscription
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Push([Bind("PushEndpoint,PushP256DH,PushAuth,EmployeeID")] Subscription sub, string btnSubmit)
        {
            if (btnSubmit == "Unsubscribe")//Delete the subscription record
            {
                try
                {
                    var sToRemove = _context.Subscriptions.Where(s => s.PushAuth == sub.PushAuth
                        && s.PushEndpoint == sub.PushEndpoint
                        && s.PushAuth == sub.PushAuth).FirstOrDefault();
                    if (sToRemove != null)
                    {
                        _context.Subscriptions.Remove(sToRemove);
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Error: Could not remove the record of the subscription.");
                }
            }
            else//Create the subscription record
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        _context.Add(sub);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Error: Could not create the record of the subscription.");
                }
            }
            ViewData["EmployeeID"] = sub.EmployeeID;
            return View(sub);
        }

        public IActionResult GenerateKeys()
        {
            var keys = VapidHelper.GenerateVapidKeys();
            ViewBag.PublicKey = keys.PublicKey;
            ViewBag.PrivateKey = keys.PrivateKey;
            return View();
        }

        // GET: EmployeeAccount/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EmployeeAccount/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Phone,FavouriteIceCream,Email")] Employee employee)
        {

            employee.Email = User.Identity.Name;
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(employee);
                    await _context.SaveChangesAsync();
                    UpdateUserNameCookie(employee.FullName);
                    return RedirectToAction(nameof(Details));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(employee);
        }

        // GET: EmployeeAccount/Edit/5
        public async Task<IActionResult> Edit()
        {
            var employee = await _context.Employees
                .Where(c => c.Email == User.Identity.Name)
                .FirstOrDefaultAsync();
            if (employee == null)
            {
                return RedirectToAction(nameof(Create));
            }
            return View(employee);
        }

        // POST: EmployeeAccount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var employeeToUpdate = await _context.Employees
                .FirstOrDefaultAsync(m => m.ID == id);

            if (await TryUpdateModelAsync<Employee>(employeeToUpdate, "",
                c => c.FirstName, c => c.LastName, c => c.Phone))
            {
                try
                {
                    _context.Update(employeeToUpdate);
                    await _context.SaveChangesAsync();
                    UpdateUserNameCookie(employeeToUpdate.FullName);
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employeeToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. The record you attempted to edit "
                                + "was modified by another user after you received your values.  You need to go back and try your edit again.");
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Something went wrong in the database.");
                }
            }
            return View(employeeToUpdate);

        }

        //// GET: EmployeeAccount/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var employee = await _context.Employees
        //        .FirstOrDefaultAsync(m => m.ID == id);
        //    if (employee == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(employee);
        //}

        //// POST: EmployeeAccount/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var employee = await _context.Employees.FindAsync(id);
        //    _context.Employees.Remove(employee);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private void UpdateUserNameCookie(string userName)
        {
            CookieHelper.CookieSet(HttpContext, "userName", userName, 960);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }
    }
}
