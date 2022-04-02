using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class EmployeesController : Controller
    {
        private readonly GROWContext _context;
        private readonly ApplicationDbContext _identityContext;
        //for sending email
        private readonly IMyEmailSender _emailSender;

        public EmployeesController(GROWContext context, ApplicationDbContext identityContext, IMyEmailSender emailSender)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees
                .Include(e=>e.Subscriptions).ToListAsync());
        }

        // GET: Employee/Create
        public IActionResult Create()
        {
            Employee employee = new Employee();
            return View(employee);
        }

        // POST: Employee/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Phone,FavouriteIceCream,Email")] Employee employee)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(employee);
                    await InviteUserToRegister(employee, null);
                    await _context.SaveChangesAsync();

                    //Send Email to new Employee - commented out till email configured
                    //await InviteUserToRegister(employee, null);

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(employee);
        }

        private async Task InviteUserToRegister(Employee employee, string message)
        {
            message ??= "Hello " + employee.FirstName + "<br /><p>Please navigate to:<br />" +
                        "<a href='https://grow-pos.azurewebsites.net/' title='https://grow-pos.azurewebsites.net/' target='_blank' rel='noopener'>" +
                        "https://grow-pos.azurewebsites.net</a><br />" +
                        " and Register using " + employee.Email + " for email address.</p>";
            //Sending the email commented out until the service is configured.
            await _emailSender.SendOneAsync(employee.FullName, employee.Email,
                "Account Registration", message);
            TempData["message"] = "Invitation email sent to " + employee.FullName + " at " + employee.Email;

        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, bool Active)
        {
            var employeeToUpdate = await _context.Employees
                .FirstOrDefaultAsync(m => m.ID == id);
            if (employeeToUpdate == null)
            {
                return NotFound();
            }

            //Note the current Email and Active Status
            bool ActiveStatus = employeeToUpdate.Active;
            string currentEmail = employeeToUpdate.Email;

            if (await TryUpdateModelAsync<Employee>(employeeToUpdate, "",
                e => e.FirstName, e => e.LastName, e => e.Phone, e => e.Email, e => e.Active))
            {
                try
                {
                    await _context.SaveChangesAsync();

                    //Delete Login if you are making them inactive
                    if (employeeToUpdate.Active == false && ActiveStatus == true)
                    {
                        //This deletes the user's login from the security system
                        await DeleteIdentityUser(employeeToUpdate.Email);

                    }
                    //Delete old Login if you Changed the email
                    if (employeeToUpdate.Email != currentEmail)
                    {
                        //This deletes the user's login from the security system
                        await DeleteIdentityUser(currentEmail);
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employeeToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Email", "Unable to save changes. Remember, you cannot have duplicate Email addresses.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            return View(employeeToUpdate);
        }

        public IActionResult Roles()
        {
            var RolesU = _context.RolesWithUsers
                .AsNoTracking()
                .ToList();
            return View(RolesU);
        }

        private async Task DeleteIdentityUser(string Email)
        {
            var userToDelete = await _identityContext.Users.Where(u => u.Email == Email).FirstOrDefaultAsync();
            if (userToDelete != null)
            {
                _identityContext.Users.Remove(userToDelete);
                await _identityContext.SaveChangesAsync();
            }
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }
    }
}
