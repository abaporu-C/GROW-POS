using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class HouseholdInformation
    {
        [Display(Name = "Household Number")]
        public int Code { get; set; }

        [Display(Name = "Member")]
        public string Name { get; set; }

        public string Gender { get; set; }

        public string Age { get; set; }

        [Display(Name = "Total Household Income")]
        [DataType(DataType.Currency)]
        public double TotalIncome { get; set; }
    }
}