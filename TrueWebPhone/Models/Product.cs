using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrueWebPhone.Models
{
    public class Product
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string ProductName { get; set; }

        public Product()
        {
            ProductOrders = new List<ProductOrder>();
        }

        public string Barcode { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public string Category { get; set; }
        public DateTime CreationDate { get; set; }

        public Boolean isSelled { get; set; }
        
        public ICollection<ProductOrder> ProductOrders { get; set; }

    }
}
