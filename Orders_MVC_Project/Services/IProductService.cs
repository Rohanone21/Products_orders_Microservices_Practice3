using Orders_MVC_Project.DTO_s;
using Orders_MVC_Project.Models;

namespace Orders_MVC_Project.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>?> GetProductsAsync();
    }
}
