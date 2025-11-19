namespace OrderService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int? ProductId { get; set; } // This is nullable
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    }
}
