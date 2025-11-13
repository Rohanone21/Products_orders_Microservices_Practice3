using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Client
{
    public interface IProductApiClient
    {
        Task<Product?> GetProductByIdAsync(int id);
    }
    public class ProductApiClient: IProductApiClient
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
                _logger.LogInformation("Fetching product with ID: {ProductId}", id);
                var response = await _client.GetAsync($"/api/products/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch product with ID: {ProductId}. Status: {StatusCode}",
                        id, response.StatusCode);
                    return null;
                }
                var content = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<Product>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                _logger.LogInformation("Successfully fetched product: {ProductName}", product?.Name);
                return product;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred while fetching product with ID: {ProductId}", id);
                return null;

            }
        }
    }
}
