namespace Orders_New_MVC_Microservice.Models.ViewModels
{
    public class StatsViewModel
    {
        public int TotalOrders { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime LatestOrderDate { get; set; }
    }
}
