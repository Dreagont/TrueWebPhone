using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace TrueWebPhone.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a product name.")]
        public string ProductName { get; set; }

        public string Barcode { get; set; }

        [Required(ErrorMessage = "Import Price must be greater than or equal to 0.")]
        [Range(0, double.MaxValue, ErrorMessage = "Import Price must be greater than or equal to 0.")]
        public decimal ImportPrice { get; set; }

        [Required(ErrorMessage = "Retail Price must be greater than or equal to 0.")]
        [Range(0, double.MaxValue, ErrorMessage = "Retail Price must be greater than or equal to 0.")]
        public decimal RetailPrice { get; set; }

        public string Category { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsSelled { get; set; }
        public int Quantity { get; set; }

        [NotMapped] // This property will not be mapped to the database
        [Display(Name = "Choose a picture for your product")]
        public IFormFile ProductImage { get; set; }

        // Remove [Required] attribute to allow null or empty value during file upload processing
        public string ImagePath { get; set; }
       
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ImportPrice >= RetailPrice)
            {
                yield return new ValidationResult("Import Price must be lower than Retail Price.", new[] { nameof(ImportPrice), nameof(RetailPrice) });
            }
        }

    }
}
