using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class IncomeSituation
    {
        //Constructor
        public IncomeSituation()
        {
            MemberIncomeSituations = new HashSet<MemberIncomeSituation>();
        }

        //Fields
        public int ID { get; set; }

        [Display(Name = "Program Name")]
        public string Situation { get; set; }

        //O:M Relationships
        public ICollection<MemberIncomeSituation> MemberIncomeSituations { get; set; }
    }
}