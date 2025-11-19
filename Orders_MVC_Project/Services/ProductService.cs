using Orders_MVC_Project.Models;
using System.Text.Json;

namespace Orders_MVC_Project.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<ProductService> _logger;

        public ProductService(HttpClient httpClient, ILogger<ProductService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching products directly from ProductService API...");

                var response = await _httpClient.GetAsync("api/products");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<Product>>(content, _jsonOptions) ?? new List<Product>();

                    _logger.LogInformation("Successfully fetched {Count} products from ProductService", products.Count);
                    return products;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch products. Status: {StatusCode}", response.StatusCode);
                    return new List<Product>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from ProductService");
                return new List<Product>();
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching product {ProductId} from ProductService", id);

                var response = await _httpClient.GetAsync($"api/products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<Product>(content, _jsonOptions);
                    return product;
                }
                else
                {
                    _logger.LogWarning("Product {ProductId} not found. Status: {StatusCode}", id, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId}", id);
                return null;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing connection to ProductService...");

                var response = await _httpClient.GetAsync("api/products");

                _logger.LogInformation("Connection test response: {StatusCode}", response.StatusCode);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProductService connection test failed");
                return false;
            }
        }
    }
}
