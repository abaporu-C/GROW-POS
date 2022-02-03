using GROW_CRM.Models.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Message : Auditable 
    {
        public Message()
        {
            Notifications = new HashSet<Notification>();
        }

        //Fields
        public int ID { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public System.DateTime Date { get; set; }

        //O:M Relationships
        public ICollection<Notification> Notifications { get; set; }
    }
}