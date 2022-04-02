using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        [StringLength(512)]
        public string PushEndpoint { get; set; }

        [StringLength(512)]
        public string PushP256DH { get; set; }

        [StringLength(512)]
        public string PushAuth { get; set; }


        [Required(ErrorMessage = "You must select the Staff Member.")]
        [Display(Name = "Employee")]
        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }
    }
}
