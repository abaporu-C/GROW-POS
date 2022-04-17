using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GROW_CRM.Areas.Identity.Pages.Account
{
    [Authorize(Roles="SuperAdmin")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IMyEmailSender _emailSender;
        private readonly GROWContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IMyEmailSender emailSender, GROWContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Display(Name = "First Name")]
            [Required(ErrorMessage = "You cannot leave the first name blank.")]
            [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            [Required(ErrorMessage = "You cannot leave the last name blank.")]
            [StringLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
            public string LastName { get; set; }

            [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
            [DataType(DataType.PhoneNumber)]
            [StringLength(10)]
            public string Phone { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = "/Employees")
        {            
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                try
                {
                    //Create Employee
                    Employee newEmp = new Employee { FirstName = Input.FirstName, LastName = Input.LastName, Phone = Input.Phone ?? "", Email = Input.Email, Active = true };

                    _context.Add(newEmp);
                    await _context.SaveChangesAsync();
                    //First see if the user exists and is allowed to register
                    var emp = _context.Employees.Where(e => e.Email == Input.Email).FirstOrDefault();
                    if (emp == null)
                    {
                        string msg = "Error: Account for " + Input.Email + " has not been created by the Admin.";
                        _logger.LogInformation(msg);
                        ViewData["msg"] = msg;
                    }
                    else if (!emp.Active)
                    {
                        string msg = "Error: Account for login " + Input.Email + " is not active.";
                        _logger.LogInformation(msg);
                        ViewData["msg"] = msg;
                    }
                    else //All good to add the account
                    {
                        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                        var result = await _userManager.CreateAsync(user, Input.Password);
                        if (result.Succeeded)
                        {
                            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = Url.Page(
                                "/Account/ResetPassword",
                                pageHandler: null,
                                values: new { area = "Identity", code },
                                protocol: Request.Scheme);

                            await InviteUserToRegister(newEmp, callbackUrl);

                            string msg = "Success: Account for " + Input.Email + " has been created with password.";
                            _logger.LogInformation(msg);
                            ViewData["msg"] = msg;
                            //await _signInManager.SignInAsync(user, isPersistent: false);
                            //Set Cookie to show full name
                            //CookieHelper.CookieSet(HttpContext, "userName", emp.FullName, 3200);
                            return LocalRedirect(returnUrl);
                        }
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "Unable to save changes. Remember, the email account and user name informations are required and you can't use the same email address to add two employees.");
                }                
            }
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task InviteUserToRegister(Employee employee, string url)
        {
            string message = "Hello " + employee.FirstName + "<br /><p>Please navigate to:<br />" +
                        $"<a href='{url}' title='{url}' target='_blank' rel='noopener'>" +
                        $"{url}</a><br />" +
                        " and Register your own password using " + employee.Email + " for email address.</p>";
            //Sending the email commented out until the service is configured.
            await _emailSender.SendOneAsync(employee.FullName, employee.Email,
                "Account Registration", message);
            TempData["message"] = "Invitation email sent to " + employee.FullName + " at " + employee.Email;

        }
    }
}
