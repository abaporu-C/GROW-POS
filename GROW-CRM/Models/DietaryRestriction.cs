using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class DietaryRestriction
    {
        //Constructor
        public DietaryRestriction()
        {
            DietaryRestrictionMembers = new HashSet<DietaryRestrictionMember>();
        }

        //Fields
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the Restriction blank")]
        [StringLength(50, ErrorMessage = "Name cannot be more than 50 characters long.")]
        public string Restriction { get; set; }

        //Foreign Keys
        public int HealthIssueTypeID { get; set; }

        public HealthIssueType HealthIssueType { get; set; }

        //O:M Relationships
        public ICollection<DietaryRestrictionMember> DietaryRestrictionMembers { get; set; }
    }
}