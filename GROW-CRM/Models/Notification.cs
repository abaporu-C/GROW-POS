using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Notification
    {
        //Constructor
        public Notification()
        {
            HouseholdNotifications = new HashSet<HouseholdNotification>();
            Members = new HashSet<Member>();
        }


        //Fields
        public int ID { get; set; }

        //Foreign Keys
        [Display(Name = "Notification Type")]
        public int NotificationTypeID { get; set; }

        [Display(Name = "Notification Type")]
        public NotificationType NotificationType { get; set; }

        [Display(Name = "Subject")]
        [StringLength(80, ErrorMessage = "Subject cannot be more than 80 characters long.")]
        public string subject { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }

        public DateTime Date = DateTime.Today;

        //public int MessageID { get; set; }

        //public Message Message { get; set; }

        //O:M Relationships
        public ICollection<HouseholdNotification> HouseholdNotifications { get; set; }
        public ICollection<Member> Members { get; set; }
    }
}