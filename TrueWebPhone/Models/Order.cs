using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrueWebPhone.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string OrderNumber { get; set; }

        [Required]
        public string CustomerId { get; set; }
        [Required]
        public Customer Customer { get; set; }
        [Required]
        public string StaffId { get; set; }
        [Required]
        public int TaxRate { get; set; }
        [Required]
        public int TaxFee { get; set; }
        [Required]
        public int SubTotal { get; set; }
        [Required]
        public int Cash { get; set; }
        [Required]
        public int Change { get; set; }
        [Required]
        public int Total { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int PaymentMethod { get; set; }
        [Required]
        public string CreatedDate { get; set; }
        public ICollection<ProductOrder> ProductOrders { get; set; }

    }
}
