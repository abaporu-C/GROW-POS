using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Province
    {
        //Constructors
        public Province()
        {
            Households = new HashSet<Household>();
            Abouts = new HashSet<About>();
        }

        //Fields
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the Code blank")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "You cannot leave the Name blank")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Name { get; set; }

        //O:M Relationships
        public ICollection<Household> Households { get; set; }
        public ICollection<About> Abouts { get; set; }
    }
}