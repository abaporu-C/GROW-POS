using GROW_CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.ViewModels
{
    public class YearlyReportVM
    {
        public int ID { get; set; }

        [Display(Name = "Household")]
        public string Code { get; set; }

        public string Members { get; set; }

        public string Address { get; set; }

        public DateTime LastVerification { get; set; }

        [Display(Name = "Pending Reassessment")]
        public string PendingReassessment { get; set; }
    }
}
