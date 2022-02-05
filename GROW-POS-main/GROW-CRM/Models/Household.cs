using GROW_CRM.Models.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Household : Auditable
    {
        //Constructor
        public Household()
        {
            Members = new HashSet<Member>();
            HouseholdDocuments = new HashSet<HouseholdDocument>();
            HouseholdNotifications = new HashSet<HouseholdNotification>();            
        }

        //Fields
        public int ID { get; set; }

        [Display(Name ="Full Address")]
        public string FullAddress
        {
            get
            {
                return StreetNumber + " " + StreetName + ", Unit " + AptNumber + "- " + City + ", " + Province.Name + " " + PostalCode;
            }
        }

        [Display(Name = "H. Code")]
        public string HCode
        {
            get
            {
                return HouseholdCode.ToString().PadLeft(5, '0');
            }
        }


        [Display(Name="Street Number")]
        [Required(ErrorMessage = "You cannot leave the Street Number blank.")]
        public int StreetNumber { get; set; }

        [Display(Name ="Street Name")]
        [Required(ErrorMessage ="You can not leave the Street Name blank")]
        [StringLength(100)]
        public string StreetName { get; set; }

        [Display(Name ="Apartment Number")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Apartment number must be numeric")]
        public int? AptNumber { get; set; }

        [Display(Name = "City Name")]
        [Required(ErrorMessage = "You cannot leave the name of the city blank.")]
        [StringLength(255, ErrorMessage = "City name cannot be more than 255 characters long.")]
        public string City { get; set; }

        [Display(Name ="Postal Code")]
        [Required(ErrorMessage ="You cannot leave the Postal Code blank")]
        [RegularExpression("[ABCEGHJKLMNPRSTVXY][0-9][ABCEGHJKLMNPRSTVWXYZ] ?[0-9][ABCEGHJKLMNPRSTVWXYZ][0-9]")]
        public string PostalCode { get; set; }

        [Display(Name = "Household Code")]
        [Required(ErrorMessage = "The 5 digit Code for the Household is required")]
        [RegularExpression("^\\d{5}$", ErrorMessage = "The Household Code must be exactly 5 numeric digits.")]
        [StringLength(5)]//DS Note: we only include this to limit the size of the database field to 10
        public string HouseholdCode { get; set; }

        [Display(Name ="Yearly Income")]
        [Required(ErrorMessage ="You cannot leave the Yearly Income blank")]
        [Range(0.1d, 999999999.99d, ErrorMessage = "The yearly income cannot exceed 999,999,999.99")]
        public decimal YearlyIncome { get; set; }

        [Display(Name ="Members in the Household")]
        [Required(ErrorMessage ="You must provide the number of people living in you household")]
        [Range(1,10,ErrorMessage ="Members in the household cannot exceed 10 people")]
        public int NumberOfMembers { get; set; }

        [Display(Name ="LICO verification")]
        public bool LICOVerified { get; set; }

        [Display(Name ="Join Date")]
        public DateTime JoinedDate { get; set; } = DateTime.Now;

        //Foreign Keys
        //[Display(Name ="City")]

        [Display(Name = "Province")]
        [Required(ErrorMessage = "You must select a Province")]
        public int ProvinceID { get; set; }

        public Province Province { get; set; }

        [Display(Name ="Household Status")]
        [Required(ErrorMessage ="A Status should be selected")]
        public int HouseholdStatusID { get; set; }

        public HouseholdStatus HouseholdStatus { get; set; }


        //O:M Relationships

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }//Added for concurrency

        public ICollection<Member> Members { get; set; }

        public ICollection<HouseholdDocument> HouseholdDocuments { get; set; }

        public ICollection<HouseholdNotification> HouseholdNotifications { get; set; }  
    }
}