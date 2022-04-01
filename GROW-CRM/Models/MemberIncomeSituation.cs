using System;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class MemberIncomeSituation
    {
        public int ID { get; set; }

        [Display(Name = "Member")]
        public int? MemberID { get; set; }
        public Member Member { get; set; }

        [Display(Name = "Select Financial Assistance Program")]
        [Required(ErrorMessage = "You must select one Financial Assistance")]
        [Range(1, int.MaxValue, ErrorMessage = "You must choose the Financial Assistance")]
        public int IncomeSituationID { get; set; }
        public IncomeSituation IncomeSituation { get; set; }

        [Display(Name = "Income Situation")]
        public string Summary
        {
            get
            {
                return IncomeSituation?.Situation + ": " + Income.ToString("c");
            }
        }

        [Display(Name = "Income")]
        public string IncomeFormated
        {
            get
            {
                return this.Income.ToString("c");
            }
        }

        [Display(Name = "Yearly Income for the program selected (in CAD)")]
        [Required(ErrorMessage = "You must enter the amount of an Income Situation")]
        [DataType(DataType.Currency)]
        [Range(0d, 1000000d, ErrorMessage = "Income Situation must be between 0 and one million dollars")]
        public double Income { get; set; }
    }
}
