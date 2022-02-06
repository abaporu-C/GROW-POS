using System;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class RenewalReport
    {        
        public int ID { get; set; }
        
        public int Members { get; set; }
        
        public double Income { get; set; }

        public DateTime LastVerified { get; set; }
    }
}