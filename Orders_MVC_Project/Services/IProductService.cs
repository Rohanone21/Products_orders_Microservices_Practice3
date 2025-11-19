using Orders_MVC_Project.Models;

namespace Orders_MVC_Project.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<bool> TestConnectionAsync();
    }
}
