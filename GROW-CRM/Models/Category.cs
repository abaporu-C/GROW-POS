using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Category
    {
        //Constructor
        public Category()
        {
            Items = new HashSet<Item>();
        }

        //Properties
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the Name blank")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Name { get; set; }

        //Relationships
        public ICollection<Item> Items { get; set; }
    }
}
