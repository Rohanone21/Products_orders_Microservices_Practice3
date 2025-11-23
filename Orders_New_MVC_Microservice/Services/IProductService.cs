using Orders_New_MVC_Microservice.Models;

namespace Orders_New_MVC_Microservice.Services
{
    public interface IProductService
    {

        Task<List<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
    }
}
