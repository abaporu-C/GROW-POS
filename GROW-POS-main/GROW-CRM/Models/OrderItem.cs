namespace GROW_CRM.Models
{
    public class OrderItem
    {
        //Fields 
        public int ID { get; set; }

        public int Quantity { get; set; }

        //Foreign Keys
        public int OrderID { get; set; }

        public Order Order { get; set; }

        public int ItemID { get; set; }

        public Item Item { get; set; }
    }
}