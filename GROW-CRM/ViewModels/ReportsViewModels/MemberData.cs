using System;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class MemberData
    {
        public int ID { get; set; }        
        
        public string Name { get; set; }

        public string Age { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Notes { get; set; }

        public bool ConsentGiven { get; set; }

        public bool DependantMember { get; set; }

        public string YearlyIncome { get; set; }

        public string IncomeSources { get; set; }
        
        public string DietaryRestrictions { get; set; }

        //Foreign Keys        

        public string Gender { get; set; }
        
        public string Address { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
