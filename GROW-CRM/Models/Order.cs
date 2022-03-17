using GROW_CRM.Models.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Order : Auditable
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        //Fields 
        public int ID { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Taxes { get; set; }

        [DataType(DataType.Currency)]
        public decimal Total { get; set; }
        
        //Foreign Keys        

        public int MemberID { get; set; }

        public Member Member { get; set; }

        public int PaymentTypeID { get; set; }
        [Display(Name = "Payment")]
        public PaymentType PaymentType { get; set; }

        //O:M Relationships
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}