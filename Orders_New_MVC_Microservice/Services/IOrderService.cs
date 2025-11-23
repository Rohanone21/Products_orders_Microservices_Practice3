using Orders_New_MVC_Microservice.Models;
using Orders_New_MVC_Microservice.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orders_New_MVC_Microservice.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task<bool> CreateOrderAsync(Order order);
        Task<bool> MarkOrderAsPaidAsync(int id);   // ⭐ ADDED
        Task<StatsViewModel> GetStatsAsync();
        Task<List<Order>> SearchOrdersAsync(string customer);
        Task<List<Order>> GetOrdersByDateRangeAsync(DateTime start, DateTime end);
        Task<List<Order>> GetLatestOrdersAsync(int count);
        Task<MostOrderedProductViewModel> GetMostOrderedProductAsync();
        Task<List<ProductPerformanceVM>> GetProductPerformanceAsync();
        Task<List<ProductDemandViewModel>> GetProductDemandAsync();
        Task<List<object>> GetProductPerformanceDateRangeAsync(DateTime start, DateTime end);
       
    }
}
