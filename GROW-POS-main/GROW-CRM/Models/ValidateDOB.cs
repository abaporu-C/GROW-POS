using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
