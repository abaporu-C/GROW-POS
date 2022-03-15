using GROW_CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class OrdersReport
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public string Member { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public int NumberOfItems { get; set; }
    }
}
