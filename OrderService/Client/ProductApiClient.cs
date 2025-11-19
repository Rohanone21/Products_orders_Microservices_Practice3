using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Client
{
    public interface IProductApiClient
    {
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<Product>> GetProductsAsync();
        Task<bool> TestConnectionAsync();
    }

    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProductApiClient> _logger;

        public ProductApiClient(HttpClient client, ILogger<ProductApiClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _client.GetAsync($"api/products/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var product = JsonSerializer.Deserialize<Product>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return product;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product with ID: {ProductId}", id);
                return null;
            }
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var response = await _client.GetAsync("api/products");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return products ?? new List<Product>();
                }
                return new List<Product>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return new List<Product>();
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _client.GetAsync("api/products");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection");
                return false;
            }
        }
    }
}