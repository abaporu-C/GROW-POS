using System.Collections.Generic;

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

        public string Name { get; set; }

        //O:M Relationships

        public ICollection<Member> Members { get; set; }
    }
}