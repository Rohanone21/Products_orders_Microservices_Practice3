using System.Collections.Generic;

namespace Orders_New_MVC_Microservice.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<Product> Products { get; set; } = new();
        //public List<Order> Orders { get; set; } = new();

        public List<OrderWithName> Orders { get; set; } = new();
    }
}
