using GROW_CRM.Models.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        
        [Required]
        [Display(Name = "Household")]
        public int HouseholdCode { get; set; }

        [Required]
        [Display(Name = "Member")]
        public string HouseMember { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Required]
        public string Purchases { get; set; }

        public double Price { get; set; }

        [Required]
        public string Payment { get; set; }

        [Required]
        public string Volunteer { get; set; }

        [DataType(DataType.Currency)]
        public double Subtotal { get; set; }

        [DataType(DataType.Currency)]
        public double Taxes { get; set; }

        [DataType(DataType.Currency)]
        public double Total { get; set; }

        //Foreign Keys        

        public int MemberID { get; set; }
        public Member Member { get; set; }

        public int HouseholdID { get; set; }
        public Household Household { get; set; }

        public int PaymentTypeID { get; set; }
        public PaymentType PaymentType { get; set; }

        //O:M Relationships
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}