using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class OrderItem
    {
        public string Summary { 
            get
            {
                string summary = $"{Quantity}X{Item?.Name} - ${Item?.Price} each";
                return summary;
            } }

        //Fields 
        public int ID { get; set; }

        public int Quantity { get; set; }

        public int OrderID { get; set; }
        public Order Order { get; set; }

        public int ItemID { get; set; }
        public Item Item { get; set; }
    }
}