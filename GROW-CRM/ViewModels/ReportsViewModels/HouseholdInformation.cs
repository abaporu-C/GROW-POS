using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.ViewModels.ReportsViewModels
{
    public class HouseholdInformation
    {
        public int ID { get; set; }

        [Display(Name = "Household Number")]
        public int Code { get; set; }

        [Display(Name = "Member")]
        public string Name { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Age { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "Health/Dietary Concerns")]
        public string DietaryConcerns { get; set; }

        [Display(Name = "Income Source")]
        public string IncomeSource { get; set; }

        [Display(Name = "Total Household Income")]
        [DataType(DataType.Currency)]
        public double TotalIncome { get; set; }
    }
}