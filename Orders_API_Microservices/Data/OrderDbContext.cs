using Microsoft.EntityFrameworkCore;
using Orders_API_Microservices.Models;

namespace Orders_API_Microservices.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Example Orders (STATIC dates only)
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    OrderId = 1,
                    CustomerName = "Rohan",
                    ProductId = 1,
                    Quantity = 1,
                    OrderDate = new DateTime(2024, 01, 01)
                },
                new Order
                {
                    OrderId = 2,
                    CustomerName = "Rahul",
                    ProductId = 2,
                    Quantity = 2,
                    OrderDate = new DateTime(2024, 01, 02)
                }
            );
        }
    }
}
