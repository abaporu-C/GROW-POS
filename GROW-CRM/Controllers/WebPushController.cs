using GROW_CRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebPush;

namespace GROW_CRM.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class WebPushController : Controller
    {
        private readonly GROWContext _context;
        private readonly IConfiguration _configuration;

        public WebPushController(GROWContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }

        public async Task<IActionResult> Send(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Subscriptions)
                .Where(e => e.ID == id)
                .FirstOrDefaultAsync();
            return View(employee);
        }

        [HttpPost, ActionName("Send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int id, string FullName)
        {
            var payload = Request.Form["payload"];
            var subs = await _context.Subscriptions
                .Where(s => s.EmployeeID == id)
                .ToListAsync();

            string vapidPublicKey = _configuration.GetSection("VapidKeys")["PublicKey"];
            string vapidPrivateKey = _configuration.GetSection("VapidKeys")["PrivateKey"];

            int count = 0;
            foreach (var sub in subs)
            {
                var pushSubscription = new PushSubscription(sub.PushEndpoint, sub.PushP256DH, sub.PushAuth);
                var vapidDetails = new VapidDetails("mailto:youremail@example.com", vapidPublicKey, vapidPrivateKey);
                try
                {
                    var webPushClient = new WebPushClient();
                    webPushClient.SendNotification(pushSubscription, payload, vapidDetails);
                    count++;
                }
                catch (WebPushException ex)
                {
                    var statusCode = ex.StatusCode;
                    TempData["message"] = "Error Sending Notification to " + FullName +
                        ". Failed with Status Code " + (int)statusCode;
                    return RedirectToAction("Index", "Employees");
                }
            }

            string plural = "";
            if (count > 1)
            {
                plural = "s";
            }
            TempData["message"] = "Sent Notification to " + count +
                " Subscription" + plural + " for " + FullName;
            return RedirectToAction("Index", "Employees");
        }
    }

}

