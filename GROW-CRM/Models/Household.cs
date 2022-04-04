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
            HouseholdNotifications = new HashSet<HouseholdNotification>();            
        }

        public string MembersString
        {
            get
            {
                string returnString = "";

                foreach (Member item in Members)
                {
                    returnString += $"{item.FullName},";
                }

                return returnString;
            }
        }

        //Fields
        public int ID { get; set; }

        [Display(Name ="Full Address")]
        public string FullAddress
        {
            get
            {                
                return this?.StreetNumber + " " + this?.StreetName + " " + this?.AptNumber + " - " + City?.Name + ", " + Province?.Code + ", " + this?.PostalCode;
            }
        }                   

        [Display(Name = "Number of Members")]        
        public int NumberOfMembers
        {
            get
            {
                int count = 0;

                foreach (Member m in Members) 
                {
                    if(m.FirstName != "" && m.LastName != "") count++;
                } 

                return count;
            }
        }

        [Display(Name = "Verification Status")]
        public string VerificationStatus
        {
            get
            {
                DateTime now = DateTime.Now;
                int dateDiff = (now - LastVerification).Days;

                if (dateDiff >= 365) return $"Verification needed. Please check members income for LICO verification.";
                if (dateDiff >= 335) return $"Verification time is near! There is only {365 - dateDiff} days left for the next verification!";                

                return $"There are {365 - dateDiff} days until the next LICO verification.";
            }
        }

        [Display(Name = "Yearly Income")]
        public double YearlyIncome
        {
            get
            {
                double count = 0;

                foreach (Member m in Members) count += m.YearlyIncome;

                return count;
            }
        }

        [Display(Name = "Yearly Income")]
        public string YearlyIncomeFormated
        {
            get
            {
                double count = 0;

                foreach (Member m in Members) count += m.YearlyIncome;

                return count.ToString("C");
            }
        }

        [Display(Name = "Household Name")]
        [StringLength(20, ErrorMessage = "Household Name can't be longer than 20 symbols")]
        [Required]
        public string Name { get; set; }


        [Display(Name="Street Number")]
        [Required(ErrorMessage = "You cannot leave the Street Number blank.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Street number must be numeric")]
        public string StreetNumber { get; set; }

        [Display(Name ="Street Name")]
        [Required(ErrorMessage ="You can not leave the Street Name blank")]
        [StringLength(100)]
        public string StreetName { get; set; }

        [Display(Name ="Apartment Number")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Apartment number must be numeric")]
        public string AptNumber { get; set; }        

        [Display(Name ="Postal Code")]
        [Required(ErrorMessage ="You cannot leave the Postal Code blank")]
        [RegularExpression(@"[ABCEGHJKLMNPRSTVXY][0-9][ABCEGHJKLMNPRSTVWXYZ] ?[0-9][ABCEGHJKLMNPRSTVWXYZ][0-9]$", ErrorMessage = "E.g. A2A 2A2 (with a space)")]
        public string PostalCode { get; set; }                        

        [Display(Name ="LICO verification")]
        public bool LICOVerified { get; set; }
        
        public bool HasCustomLICO { get; set; }

        [Display(Name = "Yearly Verification")]
        public DateTime LastVerification { get; set; }

        //Foreign Keys
        [Display(Name ="City")]
        [Required(ErrorMessage = "You must select a City")]
        public int? CityID { get; set; }

        public City City { get; set; }

        [Display(Name = "Province")]
        [Required(ErrorMessage = "You must select a Province")]
        public int? ProvinceID { get; set; }

        public Province Province { get; set; }

        [Display(Name ="Household Status")]
        [Required(ErrorMessage ="A Status should be selected")]
        public int? HouseholdStatusID { get; set; }

        public HouseholdStatus HouseholdStatus { get; set; }

        public int? AboutID { get; set; }

        public About About { get; set; }


        //O:M Relationships        

        public ICollection<Member> Members { get; set; }        

        public ICollection<HouseholdNotification> HouseholdNotifications { get; set; }  
    }
}