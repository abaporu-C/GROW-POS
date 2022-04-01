using System;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class OrdersReport
    {
        public string TotalFormated
        {
            get
            {
                return Total.ToString("C");
            }
        }

        public int ID { get; set; }
        public DateTime Date { get; set; }
        public double Total { get; set; }
        public string Member { get; set; }
        //public ICollection<OrderItem> OrderItems { get; set; }

        //public ICollection<OrderItem> OrderItems { get; set; }
    }
}
