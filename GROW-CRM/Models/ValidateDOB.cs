using System;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class ValidateDOB : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // your validation logic
            var date = (DateTime)value;
            if (date < DateTime.Today)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Age out of range.  DOB cannot be a date in the future");
            }
        }

    }
}
