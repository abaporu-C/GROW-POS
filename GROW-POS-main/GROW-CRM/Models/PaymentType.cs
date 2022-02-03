using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class PaymentType
    {
        //Constructor
        public PaymentType()
        {
            Orders = new HashSet<Order>();
        }

        //Fields
        public int ID { get; set; }

        public string Type { get; set; }

        //O:M Relationships

        public ICollection<Order> Orders { get; set; }
    }
}