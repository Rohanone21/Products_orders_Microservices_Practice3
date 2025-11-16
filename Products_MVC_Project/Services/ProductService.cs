using Products_MVC_Project.Models;
using System.Text;
using System.Text.Json;

namespace Products_MVC_Project.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task<Product> GetProductAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(int id, Product product);
        Task DeleteProductAsync(int id);
        Task<List<Product>> SearchProductsAsync(string name);
        Task<List<Product>> GetProductsByPriceRangeAsync(double minPrice, double maxPrice);
        Task<object> GetProductStatsAsync();
        Task<List<Product>> GetTopExpensiveProductsAsync(int count);
        Task<List<Product>> GetProductsByNameStartsWithAsync(string letter);
    }
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7259/api/");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var response = await _httpClient.GetAsync("products");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Product>>(content, _jsonOptions) ?? new List<Product>();
        }

        public async Task<Product> GetProductAsync(int id)
        {
            var response = await _httpClient.GetAsync($"products/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Product>(content, _jsonOptions);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            var json = JsonSerializer.Serialize(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("products", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Product>(responseContent, _jsonOptions);
        }

        public async Task UpdateProductAsync(int id, Product product)
        {
            var json = JsonSerializer.Serialize(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"products/{id}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProductAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"products/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Product>> SearchProductsAsync(string name)
        {
            var response = await _httpClient.GetAsync($"products/search?name={Uri.EscapeDataString(name)}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Product>>(content, _jsonOptions) ?? new List<Product>();
        }

        public async Task<List<Product>> GetProductsByPriceRangeAsync(double minPrice, double maxPrice)
        {
            var response = await _httpClient.GetAsync($"products/price-range?minPrice={minPrice}&maxPrice={maxPrice}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Product>>(content, _jsonOptions) ?? new List<Product>();
        }

        public async Task<object> GetProductStatsAsync()
        {
            var response = await _httpClient.GetAsync("products/stats");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<object>(content, _jsonOptions);
        }

        public async Task<List<Product>> GetTopExpensiveProductsAsync(int count)
        {
            var response = await _httpClient.GetAsync($"products/Top-Expensive?count={count}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Product>>(content, _jsonOptions) ?? new List<Product>();
        }

        public async Task<List<Product>> GetProductsByNameStartsWithAsync(string letter)
        {
            var response = await _httpClient.GetAsync($"products/name-starts-with/{letter}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Product>>(content, _jsonOptions) ?? new List<Product>();
        }
    }
}
