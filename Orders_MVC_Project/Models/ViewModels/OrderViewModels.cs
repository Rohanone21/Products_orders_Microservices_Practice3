using Orders_MVC_Project.DTO_s;

namespace Orders_MVC_Project.ViewModels
{
    public class OrderListViewModel
    {
        public IEnumerable<OrderDto> Orders { get; set; }
    }

    public class OrderDetailsViewModel
    {
        public OrderDto Order { get; set; }
        public ProductDto Product { get; set; }
    }

    public class CreateOrderViewModel
    {
        public CreateOrderRequest Order { get; set; }
        public IEnumerable<ProductDto> Products { get; set; }
    }
}
