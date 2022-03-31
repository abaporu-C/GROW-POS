using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "You cannot leave the Type blank")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Type { get; set; }

        //O:M Relationships

        public ICollection<Notification> Notifications { get; set; }
    }
}