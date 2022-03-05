using System;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class NewAdditionsReport
    {
        public int ID { get; set; }
        public int Members { get; set; }
        public double Income { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; }
    }
}