using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GROW_CRM.ViewModels
{
    public class Sales
    {
        public int ID { get; set; }

        public string Household { get; set; }

        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }

        public string Purchases { get; set; }

        public double Taxes { get; set; }

        public double Total { get; set; }

        public string Volunteer { get; set; }
    }
}
