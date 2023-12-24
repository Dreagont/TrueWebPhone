namespace TrueWebPhone.Models
{
    public class CartItem
    {
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public string ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
