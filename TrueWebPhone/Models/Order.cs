using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrueWebPhone.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; }

        [Required]
        public int CustomerId { get; set; }
        [Required]
        public int StaffId { get; set; }   
        [Required]
        public decimal Cash { get; set; }
        [Required]
        public decimal Change { get; set; }
        [Required]
        public decimal Total { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
        [Required]
        public string CreatedDate { get; set; }


    }
}
