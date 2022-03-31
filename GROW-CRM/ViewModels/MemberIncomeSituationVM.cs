using GROW_CRM.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.ViewModels
{
    public class MemberIncomeSituationVM
    {
        public int ID { get; set; }

        [Display(Name = "IncomeSituation")]
        [Required(ErrorMessage = "You must select the Income Situation")]
        [Range(1, int.MaxValue, ErrorMessage = "You must choose the Income Situation")]
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

        [Required(ErrorMessage = "You must enter the amount of an Income Situation")]
        [DataType(DataType.Currency)]
        [Range(0d, 1000000d, ErrorMessage = "Income Situation must be between 0 and one million dollars")]
        public double Income { get; set; }
    }
}
