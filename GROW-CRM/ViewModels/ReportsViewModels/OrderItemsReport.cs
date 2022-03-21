using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class OrderItemsReport
    {
        public int ID { get; set; }
        public int Quantity { get; set; }
        public string Item { get; set; }
    }
}
