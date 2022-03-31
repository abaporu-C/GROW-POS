using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class About
    {
        //Constructor
        public About()
        {
            Households = new HashSet<Household>();
        }

        public int ID { get; set; }

        [Display(Name = "Organization Name")]
        [Required(ErrorMessage = "You cannot leave the name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 100 characters long.")]
        public string OrgName { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10)]
        public string PhoneNumber { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Email Address is required.")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Web Site")]
        [Required(ErrorMessage = "Web Site is required.")]
        [StringLength(255)]
        [DataType(DataType.Url)]
        public string WebSite { get; set; }

        [Display(Name = "Street Number")]
        [Required(ErrorMessage = "You cannot leave the Street Number blank.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Street number must be numeric")]
        public string StreetNumber { get; set; }

        [Display(Name = "Street Name")]
        [Required(ErrorMessage = "You can not leave the Street Name blank")]
        [StringLength(100)]
        public string StreetName { get; set; }

        [Display(Name = "Apartment Number")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Apartment number must be numeric")]
        public string AptNumber { get; set; }

        [Display(Name = "Postal Code")]
        [Required(ErrorMessage = "You cannot leave the Postal Code blank")]
        [RegularExpression(@"[ABCEGHJKLMNPRSTVXY][0-9][ABCEGHJKLMNPRSTVWXYZ] ?[0-9][ABCEGHJKLMNPRSTVWXYZ][0-9]$", ErrorMessage = "E.g. A2A 2A2 (with a space)")]
        public string PostalCode { get; set; }

        //Foreign Keys
        [Display(Name = "City")]
        [Required(ErrorMessage = "You must select a City")]
        public int? CityID { get; set; }

        public City City { get; set; }

        [Display(Name = "Province")]
        [Required(ErrorMessage = "You must select a Province")]
        public int? ProvinceID { get; set; }

        public Province Province { get; set; }
        //0:M Relationships
        public ICollection<Household> Households { get; set; }

    }
}
