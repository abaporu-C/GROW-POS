using System.Collections;
using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class City
    {
        public City()
        {
            Households = new HashSet<Household>();
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<Household> Households { get; set; }
    }
}