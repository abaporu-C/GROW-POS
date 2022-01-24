namespace GROW_CRM.Models
{
    public class HouseholdNotification
    {        
        //Composite Keys

        public int HouseholdID { get; set; }

        public Household Household { get; set; }

        public int NotificationID { get; set; }

        public Notification Notification { get; set; }
    }
}