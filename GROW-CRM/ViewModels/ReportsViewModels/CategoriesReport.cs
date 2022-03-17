using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class CategoriesReport
    {
        public int ID { get; set; }
        public string Category { get; set; }
        public double Percentage { get; set; }
        public string PercentageText { get; set; }
        public int Total { get; set; }

        public DateTime? CreatedOn { get; set; }
    }
}
