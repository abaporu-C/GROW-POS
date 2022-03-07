using System.Collections.Generic;

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

        public string Situation { get; set; }

        //O:M Relationships
        public ICollection<MemberIncomeSituation> MemberIncomeSituations { get; set; }
    }
}