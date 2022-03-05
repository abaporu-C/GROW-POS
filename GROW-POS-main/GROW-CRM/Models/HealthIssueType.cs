using System.Collections.Generic;

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
        public string Type { get; set; }

        //O:M relationships
        public ICollection<DietaryRestriction> DietaryRestrictions { get; set; }
    }
}