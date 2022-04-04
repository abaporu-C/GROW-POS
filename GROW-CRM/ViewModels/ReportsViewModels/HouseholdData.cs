using System;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class HouseholdData
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int NumberOfMembers { get; set; }

        public string Address { get; set; }

        public bool LICOVerified { get; set; }

        public bool HasCustomLICO { get; set; }

        public DateTime LastVerification { get; set; }      

        public string HouseholdStatus { get; set; }

        public string Members { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public string LastUpdatedBy { get; set; }
    }
}
