using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Pet_Management_System.Models
{
    public enum GenderEnum
    {
        Male,
        Female
    }
    public enum ColourEnum
    {
        Black,
        White,
        Brown,
        Gray,
        Golden,
        Cream
    }

    public class Product
    {
        public int Id { get; set; }
        [DisplayName("Pet Name")]
        [Required(ErrorMessage = "Pet name is required")]
        [MaxLength(45, ErrorMessage = "The maximum length must be upto 45 characters only")]
        public string Name { get; set; }
        public Decimal Price { get; set; }
        public string picture { get; set; }
        public string Description { get; set; }
        public int Age { get; set; }
        public GenderEnum SelectedGender { get; set; }
        public ColourEnum SelectedColor { get; set; }
        
        public string Gender { get; set; }
        public string Colour { get; set; }
        [Display(Name = "Updated At")]
        [Column(TypeName = "datetime2")]
        public DateTime LastUpdated { get; set; }
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<OrderedProduct> OrderedProducts { get; set; }

    }
}