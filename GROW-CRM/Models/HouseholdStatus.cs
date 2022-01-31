using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.Models
{
    public class HouseholdStatus
    {
        //Constructor
        public HouseholdStatus()
        {
            Households = new HashSet<Household>();
        }


        public int ID { get; set; }

        public string Name { get; set; }

        //0:M Relationships
        public ICollection<Household> Households { get; set; }
    }
}
