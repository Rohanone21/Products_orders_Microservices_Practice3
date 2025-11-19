namespace OrderService.Models.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public ProductDto Product { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class CreateOrderRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateOrderRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

