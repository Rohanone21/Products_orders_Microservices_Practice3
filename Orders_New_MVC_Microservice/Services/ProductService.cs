using Orders_New_MVC_Microservice.Models;
using System.Net.Http.Json;

namespace Orders_New_MVC_Microservice.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _client;

        public ProductService(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://localhost:7259/api/Products/");
        }

        // ------------------- GET ALL PRODUCTS -------------------
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = await _client.GetFromJsonAsync<List<Product>>("");
            return products ?? new List<Product>();
        }

        // ------------------- GET PRODUCT BY ID -------------------
        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _client.GetFromJsonAsync<Product>($"{id}");
            return product;
        }
    }
}





