using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class OrderItem
    {
        public int ID { get; set; }

        public int OrderID { get; set; }
        public Order Order { get; set; }

        public int ItemID { get; set; }
        public Item Item { get; set; }
    }
}