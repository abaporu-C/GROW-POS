using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class NewItemsReport
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
