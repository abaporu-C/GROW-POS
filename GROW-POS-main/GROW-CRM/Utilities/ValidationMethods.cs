using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Utilities
{
    public class ValidationMethods : ValidationAttribute
    {
        public static ValidationResult ValidateGreaterOrEqualToZero(double? value, ValidationContext context)
        {
            bool isValid = true;

            if (value.HasValue && value < 0)
            {
                isValid = false;
            }

            if (isValid)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(
                    string.Format("The field {0} must be greater than or equal to 0.", context.MemberName),
                    new List<string>() { context.MemberName });
            }
        }
    }
}
