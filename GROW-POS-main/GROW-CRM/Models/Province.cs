using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class Province
    {
        //Constructors
        public Province()
        {
            Households = new HashSet<Household>();
        }

        //Fields
        public int ID { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        //O:M Relationships
        public ICollection<Household> Households { get; set; }
    }
}