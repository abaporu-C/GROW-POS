using GROW_CRM.Models.Utilities;
using GROW_CRM.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GROW_CRM.Models
{
    public class Item : Auditable
    {
        //Fields
        private double? _discount;

        //Constructor
        public Item()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        //Summary Properties
        [Display(Name = "Price after discount ($)")]
        [DataType(DataType.Currency)]
        public double PriceAfterDiscount
        {
            get
            {
                return Discount.HasValue ? Math.Round(Price * (1 - (Discount.Value / 100)), 3) : Price;
            }
        }

        public string PriceFormatted
        {
            get
            {
                return Price.ToString("C");
            }
        }

        //Fields 
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the Code blank.")]
        [RegularExpression("^\\d+$", ErrorMessage = "The item code can only be composed by numeric characters.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "You cannot leave the Name blank")]
        [StringLength(150, ErrorMessage = "Name cannot be more than 150 characters long.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "You cannot leave the Price blank")]
        [CustomValidation(typeof(ValidationMethods), "ValidateGreaterOrEqualToZero")]
        [DataType(DataType.Currency)]
        [Display(Name = "Price")]
        public double Price { get; set; }

        [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}")]
        [CustomValidation(typeof(ValidationMethods), "ValidateGreaterOrEqualToZero")]
        [Display(Name = "Discount (%)")]
        public double? Discount { get => _discount; set => _discount = value > 0 ? value : null; }

        //O:M Relationships
        public ICollection<OrderItem> OrderItems { get; set; }

        [Required(ErrorMessage = "You must select a Category.")]
        [Display(Name = "Category")]
        public int CategoryID { get; set; }

        public Category Category { get; set; }
    }
}