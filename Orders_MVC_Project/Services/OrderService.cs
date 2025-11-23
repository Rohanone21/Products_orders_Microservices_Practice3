using Orders_MVC_Project.DTO_s;
using Orders_MVC_Project.DTO_s;
using System.Net.Http.Json;

namespace Orders_MVC_Project.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _client;

        public OrderService(HttpClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<OrderDto>?> GetOrdersAsync()
        {
            return await _client.GetFromJsonAsync<IEnumerable<OrderDto>>("api/orders");
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            return await _client.GetFromJsonAsync<OrderDto>($"api/orders/{id}");
        }

        public async Task<bool> CreateOrderAsync(CreateOrderRequest dto)
        {
            var response = await _client.PostAsJsonAsync("api/orders", dto);
            return response.IsSuccessStatusCode;
        }
    }
}
