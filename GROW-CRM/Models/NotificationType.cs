using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class NotificationType
    {
        //Constructor
        public NotificationType()
        {
            Notifications = new HashSet<Notification>();
        }

        //Fields
        public int ID { get; set; }

        public string Type { get; set; }

        //O:M Relationships

        public ICollection<Notification> Notifications { get; set; }
    }
}