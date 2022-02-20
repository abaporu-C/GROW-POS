using GROW_CRM.Data;
using GROW_CRM.Models;
using GROW_CRM.ViewModels.ReportsViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Controllers.Helpers
{
    public static class ReportsHelper
    {
        public static List<IEnumerable> GetDemoData(GROWContext _context)
        {
            var members = from m in _context.Members
                              .Include(m => m.Gender)
                          select m;

            int memberCount = members.Count();

            var genderReport = _context.Members
                                    .Include(m => m.Gender)
                                    .GroupBy(m => new { m.Gender.Name })
                                    .Select(grp => new GenderReport
                                    {
                                        Gender = grp.Key.Name,
                                        Percentage = 0,
                                        Total = grp.Count()
                                    }).ToList();

            for (int i = 0; i < genderReport.Count(); i++)
            {
                genderReport[i].Percentage = Math.Round((double)genderReport[i].Total/memberCount, 2);
                genderReport[i].PercentageText = $"{genderReport[i].Percentage * 100}%";
            }

            DateTime now = DateTime.Now;            

            int[] totals = new int[] { 0, 0, 0, 0};

            foreach (Member m in members)
            {
                TimeSpan diff = (TimeSpan)(now - m.DOB);
                int diffYears = (int)Math.Round(diff.TotalDays / 365);

                if (diffYears < 13) totals[0] += 1;
                else if (diffYears < 19) totals[1] += 1;
                else if (diffYears < 65) totals[2] += 1;
                else totals[3] += 1;
            }

            List<AgeReport> ageReport = new List<AgeReport> {
                new AgeReport{ AgeRange = "0-12", Percentage = 0, PercentageText = "", Total = 0},
                new AgeReport{ AgeRange = "13-18", Percentage = 0, PercentageText = "", Total = 0},
                new AgeReport{ AgeRange = "19-64", Percentage = 0, PercentageText = "", Total = 0},
                new AgeReport{ AgeRange = "65+", Percentage = 0, PercentageText = "", Total = 0},
            };

            for (int i = 0; i < ageReport.Count(); i++)
            {
                ageReport[i].Total = totals[i];
                double per = Math.Round((double)(totals[i] / memberCount), 2);
                ageReport[i].Percentage = per;
                ageReport[i].PercentageText = $"{per * 100}%";
            }

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

            for (int i = 0; i < dietaryReport.Count(); i++)
            {
                dietaryReport[i].Percentage = Math.Round(dietaryReport[i].Percentage/dietaryTotal, 2);
                dietaryReport[i].PercentageText = $"{dietaryReport[i].Percentage * 100}%";
            }

            return new List<IEnumerable> { genderReport, ageReport, dietaryReport };
        }
    }
}
