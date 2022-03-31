using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class HealthIssueType
    {
        public HealthIssueType()
        {
            DietaryRestrictions = new HashSet<DietaryRestriction>();
        }

        //Fields
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the Type blank")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Type { get; set; }

        //O:M relationships
        public ICollection<DietaryRestriction> DietaryRestrictions { get; set; }
    }
}