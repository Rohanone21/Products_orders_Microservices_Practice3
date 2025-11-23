using Orders_MVC_Project.DTO_s;

namespace Orders_MVC_Project.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>?> GetOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<bool> CreateOrderAsync(CreateOrderRequest dto);
    }
}
