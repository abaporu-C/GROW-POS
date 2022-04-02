using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GROW_CRM.Models;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class HouseholdIncomeReport
    {
        public int ID { get; set; }
        public string Household { get; set; }

        [Display(Name = "Total Income")]
        public decimal Income { get; set; }

        [Display(Name = "Income Situation")]
        public string Income_Situation { get; set; }

        [Display(Name = "Number of Members")]
        public int Number_of_Members { get; set; }
    }
}
