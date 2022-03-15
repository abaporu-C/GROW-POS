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

        [Display(Name = "Number of Items")]
        public int NumberOfMembers
        {
            get
            {
                int count = 0;

                foreach (OrderItem m in OrderItems)
                {
                    count++;
                }

                return count;
            }
        }
        //Fields 
        public int ID { get; set; }

        public DateTime Date { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Taxes { get; set; }

        public decimal Total { get; set; }
        
        //Foreign Keys        

        public int MemberID { get; set; }

        public Member Member { get; set; }

        public int PaymentTypeID { get; set; }

        public PaymentType PaymentType { get; set; }

        //O:M Relationships
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}