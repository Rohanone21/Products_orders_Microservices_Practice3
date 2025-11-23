namespace Orders_New_MVC_Microservice.Models.ViewModels
{
    public class ProductPerformanceVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalOrders { get; set; }
        public int TotalQuantitySold { get; set; }
    }
}
