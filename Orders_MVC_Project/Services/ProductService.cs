using Orders_MVC_Project.DTO_s;
using Orders_MVC_Project.DTO_s;
using System.Net.Http.Json;

namespace Orders_MVC_Project.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _client;

        public ProductService(HttpClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<ProductDto>?> GetProductsAsync()
        {
            return await _client.GetFromJsonAsync<IEnumerable<ProductDto>>("api/products");
        }
    }
}
