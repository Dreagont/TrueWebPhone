namespace TrueWebPhone.Models
{
    public class ProductOrder
    {
        public string OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
