using Newtonsoft.Json;
using Orders_New_MVC_Microservice.Models;
using Orders_New_MVC_Microservice.Models.ViewModels;
using System.Net.Http.Json;

namespace Orders_New_MVC_Microservice.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _http;

        public OrderService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var response = await _http.GetAsync("https://localhost:7209/api/orders");
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Order>>(json);
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            var response = await _http.GetAsync($"https://localhost:7209/api/orders/{id}");
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Order>(json);
        }

        public async Task<bool> CreateOrderAsync(Order order)
        {
            var response = await _http.PostAsJsonAsync("https://localhost:7209/api/orders", order);
            return response.IsSuccessStatusCode;
        }

        public async Task<StatsViewModel> GetStatsAsync()
        {
            return await _http.GetFromJsonAsync<StatsViewModel>("stats");
        }

        public async Task<List<Order>> SearchOrdersAsync(string customer)
        {
            var response = await _http.GetAsync($"https://localhost:7209/api/orders/search?customer={customer}");
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Order>>(json);
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime start, DateTime end)
        {
            var response = await _http.GetAsync($"https://localhost:7209/api/orders/date-range?start={start}&end={end}");
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Order>>(json);
        }

        public async Task<List<Order>> GetLatestOrdersAsync(int count)
        {
            var response = await _http.GetAsync($"https://localhost:7209/api/orders/latest?count={count}");
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Order>>(json);
        }

        public async Task<MostOrderedProductViewModel?> GetMostOrderedProductAsync()
        {
            return await _http.GetFromJsonAsync<MostOrderedProductViewModel>("most-ordered-product");
        }

        public async Task<List<ProductPerformanceVM>> GetProductPerformanceAsync()
        {
            return await _http.GetFromJsonAsync<List<ProductPerformanceVM>>("product-performance")
                   ?? new List<ProductPerformanceVM>();
        }

        public async Task<List<ProductDemandViewModel>> GetProductDemandAsync()
        {
            return await _http.GetFromJsonAsync<List<ProductDemandViewModel>>("product-demand")
                ?? new List<ProductDemandViewModel>();
        }

        public async Task<List<object>> GetProductPerformanceDateRangeAsync(DateTime start, DateTime end)
        {
            var response = await _http.GetAsync($"https://localhost:7209/api/orders/product-performance-date-range?start={start}&end={end}");
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<object>>(json);
        }
        public async Task<bool> MarkOrderAsPaidAsync(int id)
        {
            var response = await _http.PutAsync(
                $"https://localhost:7209/api/orders/{id}/mark-paid", null);

            return response.IsSuccessStatusCode;
        }


    }
}




