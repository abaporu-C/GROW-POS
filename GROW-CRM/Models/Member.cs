using GROW_CRM.Models.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Member : Auditable, IValidatableObject
    {

        //Constructor
        public Member()
        {
            DietaryRestrictionMembers = new HashSet<DietaryRestrictionMember>();
            MemberDocuments = new HashSet<MemberDocument>();
            Orders = new HashSet<Order>();
            MemberIncomeSituations = new HashSet<MemberIncomeSituation>();
        }

        //Fields

        public int ID { get; set; }

        [Display(Name = "Member")]
        public string FullName
        {
            get
            {
                return FirstName?[0].ToString().ToUpper() + FirstName?.Substring(1)
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName?[0] + ". ").ToUpper())
                    + LastName?[0].ToString().ToUpper() + LastName?.Substring(1);
            }
        }

        public string Age
        {
            get
            {
                DateTime today = DateTime.Today;
                int? a = today.Year - DOB?.Year
                    - ((today.Month < DOB?.Month || (today.Month == DOB?.Month && today.Day < DOB?.Day) ? 1 : 0));
                return a?.ToString();
            }
        }

        [Display(Name = "Yearly Income")]
        public double YearlyIncome { 
            get 
            {
                double income = 0;

                foreach(MemberIncomeSituation mis in MemberIncomeSituations)
                {
                    income += mis.Income;
                }


                return income;
            } 
        }

        [Display (Name = "Yearly Income")]
        public string YearlyIncomeFormated
        {
            get
            {
                double income = 0;

                foreach (MemberIncomeSituation mis in MemberIncomeSituations)
                {
                    income += mis.Income;
                }


                return income.ToString("C");
            }
        }

        [Display(Name = "Age (DOB)")]
        public string AgeSummary
        {
            get
            {
                string ageSummary = "Unknown";
                if (DOB.HasValue)
                {
                    ageSummary = Age + " (" + String.Format("{0:yyyy-MM-dd}", DOB) + ")";
                }
                return ageSummary;
            }
        }

        [Display(Name = "Phone")]
        public string PhoneFormatted
        {
            get
            {
                return "(" + PhoneNumber?.Substring(0, 3) + ") " + PhoneNumber?.Substring(3, 3) + "-" + PhoneNumber?[6..];
            }
        }

        public string DietaryRestrictionsString
        {
            get
            {
                string returnString = "";

                foreach (DietaryRestrictionMember item in DietaryRestrictionMembers)
                {
                    returnString += $"{item.DietaryRestriction.Restriction},";
                }

                return returnString;
            }
        }

        public string IncomeSourceString
        {
            get
            {
                string returnString = "";

                foreach (MemberIncomeSituation item in MemberIncomeSituations)
                {
                    returnString += $"{item.IncomeSituation.Situation},";
                }

                return returnString;
            }
        }


        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; }

        [Display(Name ="Date of Birth")]
        [Required(ErrorMessage = "You cannot leave the birth date blank.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DOB { get; set; }

        [Display(Name ="Phone Number")]
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10)]
        public string PhoneNumber { get; set; }

        [Display(Name ="E-mail")]
        [Required(ErrorMessage = "Email Address is required.")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name ="Notes")]        
        [StringLength(2000, ErrorMessage = "Only 2000 characters for notes.")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }        

        [Display(Name = "Member consents giving information: ")]
        public bool ConsentGiven { get; set; }

        [Display(Name = "Is this member a Dependant?")]
        public bool DependantMember { get; set; }


        //Foreign Keys        

        [Display(Name = "Gender")]
        [Required(ErrorMessage = "You must select the Gender")]
        public int? GenderID { get; set; }

        public Gender Gender { get; set; }

        [Display(Name = "Household")]
        public int HouseholdID { get; set; }

        public Household Household { get; set; }        

        //O:M Relationships        

        [Display(Name = "Dietary Restrictions")]
        public ICollection<DietaryRestrictionMember> DietaryRestrictionMembers { get; set; }

        public ICollection<MemberDocument> MemberDocuments { get; set; }

        public ICollection<Order> Orders { get; set; }

        public ICollection<MemberIncomeSituation> MemberIncomeSituations { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Test date range for DOB
            if (DOB > DateTime.Today)
            {
                yield return new ValidationResult("DOB Cannot be a date in the future", new[] { "DOB" });
            }
        }
    }

}
