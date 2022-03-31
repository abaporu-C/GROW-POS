using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Gender
    {
        //Constructor
        public Gender()
        {
            Members = new HashSet<Member>();
        }

        //Fields

        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the Name blank")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Name { get; set; }

        //O:M Relationships

        public ICollection<Member> Members { get; set; }
    }
}