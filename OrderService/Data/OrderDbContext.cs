using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Configure relationship - ProductId is optional (nullable)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.ProductId)
                .IsRequired(false) // Explicitly mark as optional
                .OnDelete(DeleteBehavior.SetNull); // Now SET NULL will work

            // Seed initial products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Price = 55000.00m },
                new Product { Id = 2, Name = "Smartphone", Price = 25000.00m },
                new Product { Id = 3, Name = "Tablet", Price = 18000.00m },
                new Product { Id = 4, Name = "Headphones", Price = 3000.00m },
                new Product { Id = 5, Name = "Smartwatch", Price = 12000.00m }
            );
        }
    }
}