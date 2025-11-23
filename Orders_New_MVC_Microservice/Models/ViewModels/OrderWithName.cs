namespace Orders_New_MVC_Microservice.Models.ViewModels
{
    public class OrderWithName
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }

        public int Quantity { get; set; }

        // ✅ Allow assigning value from controller
        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        public bool IsPaid { get; set; }

    }
}
