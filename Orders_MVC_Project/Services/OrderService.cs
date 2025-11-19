using System.Text;
using System.Text.Json;
using Orders_MVC_Project.Models;
using Orders_MVC_Project.Models.ViewModels;

namespace Orders_MVC_Project.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync();
        Task<Order> GetOrderAsync(int id);
        Task<Order> CreateOrderAsync(CreateOrderRequest request);
        Task UpdateOrderAsync(int id, UpdateOrderRequest request);
        Task DeleteOrderAsync(int id);
        Task<object> GetOrderStatsAsync();
        Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Order>> GetRecentOrdersAsync(int count);
        Task<List<Order>> GetOrdersByProductAsync(int productId);
        Task<List<object>> GetProductPerformanceAsync();
        Task<bool> SyncProductsAsync(); // Keep only this for OrderService.API sync
    }

    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<OrderService> _logger;

        public OrderService(HttpClient httpClient, ILogger<OrderService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<bool> SyncProductsAsync()
        {
            try
            {
                _logger.LogInformation("Syncing products from ProductService via OrderService API...");
                var response = await _httpClient.PostAsync("api/orders/sync-products", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Products synced successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to sync products. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing products");
                return false;
            }
        }

        public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation("Creating order for product {ProductId}, quantity {Quantity}", request.ProductId, request.Quantity);

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/orders", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var order = JsonSerializer.Deserialize<Order>(responseContent, _jsonOptions);
                    _logger.LogInformation("Order created successfully with ID: {OrderId}", order?.Id);
                    return order;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create order. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Order creation failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order via API");
                throw;
            }
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching orders from API...");
                var response = await _httpClient.GetAsync("api/orders");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var orders = JsonSerializer.Deserialize<List<Order>>(content, _jsonOptions) ?? new List<Order>();
                    _logger.LogInformation("Retrieved {Count} orders", orders.Count);
                    return orders;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch orders. Status: {StatusCode}", response.StatusCode);
                    return new List<Order>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return new List<Order>();
            }
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching order with ID: {OrderId}", id);
                var response = await _httpClient.GetAsync($"api/orders/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var order = JsonSerializer.Deserialize<Order>(content, _jsonOptions);
                    return order;
                }
                else
                {
                    _logger.LogWarning("Order with ID {OrderId} not found. Status: {StatusCode}", id, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order with ID: {OrderId}", id);
                return null;
            }
        }

        public async Task UpdateOrderAsync(int id, UpdateOrderRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/orders/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Order update failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order with ID: {OrderId}", id);
                throw;
            }
        }

        public async Task DeleteOrderAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/orders/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Order deletion failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order with ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<object> GetOrderStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/orders/stats");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<object>(content, _jsonOptions);
                }
                return new { TotalOrders = 0, TotalRevenue = 0.0m, AverageOrderValue = 0.0m };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order statistics");
                return new { TotalOrders = 0, TotalRevenue = 0.0m, AverageOrderValue = 0.0m };
            }
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var url = $"api/orders/date-range?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Order>>(content, _jsonOptions) ?? new List<Order>();
                }
                return new List<Order>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders by date range");
                return new List<Order>();
            }
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/orders/recent?count={count}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Order>>(content, _jsonOptions) ?? new List<Order>();
                }
                return new List<Order>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent orders");
                return new List<Order>();
            }
        }

        public async Task<List<Order>> GetOrdersByProductAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/orders/product/{productId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Order>>(content, _jsonOptions) ?? new List<Order>();
                }
                return new List<Order>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders for product ID: {ProductId}", productId);
                return new List<Order>();
            }
        }

        public async Task<List<object>> GetProductPerformanceAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/orders/product-performance");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<object>>(content, _jsonOptions) ?? new List<object>();
                }
                return new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product performance data");
                return new List<object>();
            }
        }
    }
}