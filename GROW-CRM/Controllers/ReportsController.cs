using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.ViewModels.ReportsViewModels;
using GROW_CRM.ViewModels;

namespace GROW_CRM.Controllers
{
    public class ReportsController : Controller
    {
        private readonly GROWContext _context;

        public ReportsController(GROWContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            GetReportsDDLItems();

            return View();
        }

        public async Task<IActionResult> SelectedReport(string Reports)
        {
            switch(Reports){
                case "0":
                    GetRenewalReport();
                    break;
                case "1":
                    GetNewAdditions();
                    break;
                case "2":
                    GetDemographics();
                    break;
                case "3":
                    GetMapping();
                    break;
                default:
                    return View("Index");                    
            }
            return View();
        }
        
        //Get Renewal Report
        public async void GetRenewalReport()
        {
            DateTime now = DateTime.Now;

            var renewalReports =  await _context.Households
                                .Include(h => h.Members)                                
                                .Select(rr => new RenewalReport
                                {
                                    ID = rr.ID,
                                    Members = rr.Members.Count,
                                    Income = rr.YearlyIncome,
                                    LastVerified = rr.LastVerification
                                }).ToListAsync();

            List<RenewalReport> renewalReportsFiltered = new List<RenewalReport>();

            foreach(RenewalReport r in renewalReports)
            {
                int diff = (now - r.LastVerified).Days;
                if (diff < 365) continue;
                renewalReportsFiltered.Add(r);
            }

            string[] headers = new string[] { "Membership #", "Number of Members", "Yearly Income", "Last Verification"};

            ViewData["ReportType"] = "Renewal Report";
            ViewData["Count"] = renewalReportsFiltered.Count();
            ViewData["Name"] = $"Memberships up for reacessment";
            ViewBag.Headers = headers;
            ViewBag.Report = renewalReportsFiltered;

            GetReportsDDLItems();
        }

        public async void GetNewAdditions()
        {
            var newAdditions = await _context.Households
                                .Include(h => h.Members)
                                .Select(na => new NewAdditionsReport
                                {
                                    ID = na.ID,
                                    Members = na.Members.Count,
                                    Income = na.YearlyIncome,
                                    CreatedOn = na.CreatedOn,
                                    CreatedBy = na.CreatedBy
                                }).ToListAsync();

            List<NewAdditionsReport> newAdditionsfiltered = new List<NewAdditionsReport>();
            
            DateTime lastWeek = DateTime.Now.AddDays(-7);

            foreach(NewAdditionsReport na in newAdditions)
            {
                TimeSpan diff = (TimeSpan)(na.CreatedOn - lastWeek);

                if (diff.TotalDays < 7) continue;
                newAdditionsfiltered.Add(na);
            }

            string[] headers = new string[] { "Membership #", "Number of Members", "Yearly Income", "Created On", "Created By" };

            ViewData["ReportType"] = "New Memberships Report";
            ViewData["count"] = newAdditionsfiltered.Count();
            ViewData["Name"] = $"New Additions - From: {lastWeek.Month}/{lastWeek.Day}/{lastWeek.Year} To: {DateTime.Now.Month}/{DateTime.Now.Day}/{DateTime.Now.Year}";
            ViewBag.Headers = headers;
            ViewBag.Report = newAdditionsfiltered;

            GetReportsDDLItems();
        }

        public void GetDemographics()
        {
            var members = from m in _context.Members
                               .Include(m => m.Gender)
                                select m;

            int memberCount = members.Count();

            var genderReport =      _context.Members
                                    .Include(m => m.Gender)
                                    .GroupBy(m => new { m.Gender.Name})
                                    .Select(grp => new GenderReport { 
                                        Gender = grp.Key.Name,
                                        Percentage = grp.Count(), 
                                        Total = grp.Count()
                                    }).ToList();

            for(int i = 0; i < genderReport.Count(); i++)
            {
                genderReport[i].Percentage /= memberCount;
            }

            DateTime now = DateTime.Now;
            List<AgeReport> ageReport = new List<AgeReport> {
                new AgeReport{ AgeRange = "0-12", Percentage = 0, Total = 0},
                new AgeReport{ AgeRange = "13-18", Percentage = 0, Total = 0},
                new AgeReport{ AgeRange = "19-64", Percentage = 0, Total = 0},
                new AgeReport{ AgeRange = "65+", Percentage = 0, Total = 0},
            };

            foreach(Member m in members)
            {
                TimeSpan diff = (TimeSpan)(now - m.DOB);
                int diffYears = (int)Math.Round(diff.TotalDays/365);

                if (diffYears < 13) ageReport[0].Total++;
                if (diffYears < 19) ageReport[1].Total++;
                if (diffYears < 65) ageReport[2].Total++;
                else ageReport[3].Total++;
            }

            ageReport[0].Percentage = ageReport[0].Total / memberCount;
            ageReport[1].Percentage = ageReport[1].Total / memberCount;
            ageReport[2].Percentage = ageReport[2].Total / memberCount;
            ageReport[3].Percentage = ageReport[3].Total / memberCount;

            var dietaryTotal = _context.DietaryRestrictionMembers.Count();

            var dietaryReport = _context.DietaryRestrictionMembers
                                .Include(dr => dr.DietaryRestriction)
                                .GroupBy(dr => new { dr.DietaryRestriction.Restriction })
                                .Select(grp => new DietaryReport
                                {
                                    Restriction = grp.Key.Restriction,
                                    Percentage = grp.Count(),
                                    Total = grp.Count()
                                }).ToList();

            for(int i = 0; i < dietaryReport.Count(); i++)
            {
                dietaryReport[i].Percentage /= dietaryTotal;
            }

            ViewData["ReportType"] = "Demographics Report";
            ViewData["Count"] = memberCount;
            ViewData["Name"] = "Demographics";            
            ViewBag.GenderReport = genderReport;
            ViewBag.AgeReport = ageReport;
            ViewBag.DietaryReport = dietaryReport;

            GetReportsDDLItems();
        }

        public void GetMapping()
        {
            var householdCount = _context.Households.Count();

            var citiesReport = _context.Households
                        .Include(h => h.City)
                        .GroupBy(h => new { h.City.Name })
                        .Select(grp => new CitiesReport
                        {
                            Name = grp.Key.Name,
                            Percentage = grp.Count(),
                            Total = grp.Count()
                        }).ToList();

            for(int i = 0; i < citiesReport.Count(); i++)
            {
                citiesReport[i].Percentage /= householdCount;
            }

            List<List<CityReport>> cityReports = new List<List<CityReport>>();

            var cities = _context.Cities.ToList();

            foreach(City c in cities)
            {
                var h = _context.Members
                        .Include(h => h.Household).ThenInclude(hh => hh.City)
                        .Where(h => h.Household.City.Name == c.Name)
                        .GroupBy(h => new { h.Household.PostalCode, h.Household.City.Name })
                        .Select(grp => new CityReport
                        {
                            Name = grp.Key.Name,
                            PostalCode = grp.Key.PostalCode,
                            NumberOfMembers = grp.Count(),
                            TotalIncome = grp.Sum(h => h.YearlyIncome)
                        }).ToList();
                               
                cityReports.Add(h);
            }

            ViewData["ReportType"] = "Mapping Report";
            ViewData["Count"] = citiesReport.Count();
            ViewData["Name"] = "Mapping";
            ViewBag.CitiesReport = citiesReport;
            ViewBag.CityReports = cityReports;

            GetReportsDDLItems();
        }

        public void GetReportsDDLItems()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "Renewal Report", Value = "0" });
            items.Add(new SelectListItem { Text = "Weekly Additions", Value = "1" });
            items.Add(new SelectListItem { Text = "Demographics", Value = "2" });
            items.Add(new SelectListItem { Text = "Mapping", Value = "3" });

            ViewBag.Reports = items;
        }

        private bool HouseholdExists(int id)
        {
            return _context.Households.Any(e => e.ID == id);
        }
    }
}
