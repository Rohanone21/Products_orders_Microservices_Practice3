using Orders_API_Microservices.DTO_s;
using System.Net.Http.Json;

public class ProductService
{
    private readonly IHttpClientFactory _http;

    public ProductService(IHttpClientFactory http)
    {
        _http = http;
    }

    public async Task<string> GetProductName(int productId)
    {
        var client = _http.CreateClient("ProductService");

        try
        {
            var product = await client.GetFromJsonAsync<ProductDTO>(productId.ToString());
            return product?.Name ?? "Unknown Product";
        }
        catch
        {
            return "Unknown Product";
        }
    }
}
