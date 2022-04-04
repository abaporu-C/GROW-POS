using System;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class OrderData
    {
        public int ID { get; set; }

        public DateTime Date { get; set; }

        public double Total { get; set; }

        //Foreign Keys        
        public string Member { get; set; }

        public string PaymentType { get; set; }

        public string OrderItems { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

    }
}
