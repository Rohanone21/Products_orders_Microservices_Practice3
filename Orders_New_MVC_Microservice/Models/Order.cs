namespace Orders_New_MVC_Microservice.Models
{
    public class Order
    {

        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; }
        public bool IsPaid { get; set; }

    }
}
