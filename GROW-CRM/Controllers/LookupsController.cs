using GROW_CRM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GROW_CRM.Controllers
{
    public class LookupsController : Controller
    {
        private readonly GROWContext _context;

        public LookupsController(GROWContext context)
        {
            _context = context;
        }

        public IActionResult Index(string Tab)
        {
            ///Note: select the tab you want to load by passing in
            ///the ID of the tab such as MedicalTrialsTab, ConditionsTab
            ///or AppointmentReasonsTab
            ViewData["Tab"] = Tab;
            return View();
        }

        public PartialViewResult DietaryRestrictions()
        {
            ViewData["DietaryRestrictionsID"] = new
                SelectList(_context.DietaryRestrictions
                .OrderBy(a => a.Restriction), "ID", "Restriction");
            return PartialView("_DietaryRestrictions");
        }

        public PartialViewResult Genders()
        {
            ViewData["GendersID"] = new
                SelectList(_context.Genders
                .OrderBy(a => a.Name), "ID", "Name");
            return PartialView("_Genders");
        }

        public PartialViewResult Provinces()
        {
            ViewData["ProvincesID"] = new
                SelectList(_context.Provinces
                .OrderBy(a => a.Name), "ID", "Name");
            return PartialView("_Provinces");
        }

        public PartialViewResult NotificationTypes()
        {
            ViewData["NotificationTypesID"] = new
                SelectList(_context.NotificationTypes
                .OrderBy(a => a.Type), "ID", "Type");
            return PartialView("_NotificationTypes");
        }

        public PartialViewResult PaymentTypes()
        {
            ViewData["PaymentTypesID"] = new
                SelectList(_context.PaymentTypes
                .OrderBy(a => a.Type), "ID", "Type");
            return PartialView("_PaymentTypes");
        }


    }
}
