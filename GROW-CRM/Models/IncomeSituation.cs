using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class IncomeSituation
    {
        //Constructor
        public IncomeSituation()
        {
            Members = new HashSet<Member>();
        }

        //Fields
        public int ID { get; set; }

        public string Situation { get; set; }

        //O:M Relationships
        public ICollection<Member> Members { get; set; }
    }
}