using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders_API_Microservices.Data;
using Orders_API_Microservices.Models;

namespace Orders_API_Microservices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _productClientName = "ProductService";

        public OrdersController(OrderDbContext context, IHttpClientFactory httpFactory)
        {
            _context = context;
            _httpFactory = httpFactory;
        }

        // DTO to receive product from Product Microservice
        private class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public double Price { get; set; }
        }

        // Helper: fetch product name via Product Service
        private async Task<string> GetProductNameAsync(int productId)
        {
            try
            {
                var client = _httpFactory.CreateClient(_productClientName);
                var product = await client.GetFromJsonAsync<ProductDto>($"{productId}");
                return product?.Name ?? $"Product {productId}";
            }
            catch
            {
                return $"Product {productId}";
            }
        }

        // ------------------------------ BASIC CRUD ------------------------------

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return Ok(await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            order.OrderDate = order.OrderDate == default ? DateTime.Now : order.OrderDate;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Order order)
        {
            if (id != order.OrderId) return BadRequest("ID mismatch");

            var existing = await _context.Orders.FindAsync(id);
            if (existing == null) return NotFound();

            existing.CustomerName = order.CustomerName;
            existing.ProductId = order.ProductId;
            existing.Quantity = order.Quantity;
            existing.OrderDate = order.OrderDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ------------------------------ SEARCH & STATS ------------------------------

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Order>>> SearchOrders([FromQuery] string customer)
        {
            if (string.IsNullOrWhiteSpace(customer)) return BadRequest("customer required");

            var orders = await _context.Orders
                .Where(o => o.CustomerName.Contains(customer))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            if (!orders.Any()) return NotFound();
            return Ok(orders);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetOrderStats()
        {
            return Ok(new
            {
                totalOrders = await _context.Orders.CountAsync(),
                totalQuantity = await _context.Orders.SumAsync(q => q.Quantity),
                latestOrderDate = await _context.Orders.AnyAsync()
                    ? await _context.Orders.MaxAsync(o => o.OrderDate)
                    : (DateTime?)null
            });
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByDateRange(
            [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end) return BadRequest("start must be <= end");

            var endInclusive = end.Date.AddDays(1).AddTicks(-1);

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= start.Date && o.OrderDate <= endInclusive)
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<Order>>> GetLatestOrders([FromQuery] int count = 5)
        {
            return Ok(await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync());
        }

        // ------------------------------ PRODUCT ANALYTICS ------------------------------

        // Most ordered product (highest total quantity)
        [HttpGet("most-ordered-product")]
        public async Task<ActionResult> GetMostOrderedProduct()
        {
            var raw = await _context.Orders
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .FirstOrDefaultAsync();

            if (raw == null) return Ok(new { });

            return Ok(new
            {
                productId = raw.ProductId,
                productName = await GetProductNameAsync(raw.ProductId),
                totalQuantity = raw.TotalQuantity
            });
        }

        // Product performance overall
        [HttpGet("product-performance")]
        public async Task<ActionResult> GetProductPerformance()
        {
            var raw = await _context.Orders
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalOrders = g.Count(),
                    TotalQuantitySold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .ToListAsync();

            var result = new List<object>();

            foreach (var item in raw)
            {
                result.Add(new
                {
                    productId = item.ProductId,
                    productName = await GetProductNameAsync(item.ProductId),
                    totalOrders = item.TotalOrders,
                    totalQuantitySold = item.TotalQuantitySold
                });
            }

            return Ok(result);
        }

        // Product demand (just count orders)
        [HttpGet("product-demand")]
        public async Task<ActionResult> GetProductDemand()
        {
            var raw = await _context.Orders
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    OrdersCount = g.Count()
                })
                .OrderByDescending(x => x.OrdersCount)
                .ToListAsync();

            var result = new List<object>();

            foreach (var item in raw)
            {
                result.Add(new
                {
                    productId = item.ProductId,
                    productName = await GetProductNameAsync(item.ProductId),
                    ordersCount = item.OrdersCount
                });
            }

            return Ok(result);
        }

        // Product performance in date range
        [HttpGet("product-performance-date-range")]
        public async Task<ActionResult> GetProductPerformanceDateRange(
            [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end) return BadRequest("start must be <= end");

            var endInclusive = end.Date.AddDays(1).AddTicks(-1);

            var raw = await _context.Orders
                .Where(o => o.OrderDate >= start.Date && o.OrderDate <= endInclusive)
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalOrders = g.Count(),
                    TotalQuantitySold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .ToListAsync();

            var result = new List<object>();

            foreach (var item in raw)
            {
                result.Add(new
                {
                    productId = item.ProductId,
                    productName = await GetProductNameAsync(item.ProductId),
                    totalOrders = item.TotalOrders,
                    totalQuantitySold = item.TotalQuantitySold
                });
            }

            return Ok(result);
        }

        // Yesterday + Today (automatically calculated)
        [HttpGet("product-performance-today-yesterday")]
        public async Task<ActionResult> GetProductPerformanceTodayYesterday()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var endInclusive = today.AddDays(1).AddTicks(-1);

            var raw = await _context.Orders
                .Where(o => o.OrderDate >= yesterday && o.OrderDate <= endInclusive)
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalOrders = g.Count(),
                    TotalQuantitySold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .ToListAsync();

            var result = new List<object>();

            foreach (var item in raw)
            {
                result.Add(new
                {
                    productId = item.ProductId,
                    productName = await GetProductNameAsync(item.ProductId),
                    totalOrders = item.TotalOrders,
                    totalQuantitySold = item.TotalQuantitySold
                });
            }

            return Ok(result);
        }

        [HttpPut("{id}/mark-paid")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            Console.WriteLine("API HIT: mark-paid for ID = " + id);

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            order.IsPaid = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Payment updated" });
        }



    }
}
