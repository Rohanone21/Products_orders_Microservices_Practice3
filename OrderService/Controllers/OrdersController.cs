using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Client;
using OrderService.Data;
using OrderService.Models;
using OrderService.Dtos;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IProductApiClient _productApi;

        public OrdersController(OrderDbContext context, IProductApiClient productApi)
        {
            _context = context;
            _productApi = productApi;
        }

        // ---------------------------------------------------------
        // GET ALL ORDERS
        // ---------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.Product)
                .ToListAsync();
        }

        // ---------------------------------------------------------
        // GET ORDER BY ID
        // ---------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return order;
        }

        // ---------------------------------------------------------
        // CREATE ORDER
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto dto)
        {
            // Validate product
            var product = await _productApi.GetProductByIdAsync(dto.ProductId);
            if (product == null)
                return BadRequest("Product not found.");

            var order = new Order
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                TotalPrice = product.Price * dto.Quantity,
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Send product info in response (not tracked by EF)
            order.Product = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        // ---------------------------------------------------------
        // UPDATE ORDER
        // ---------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, CreateOrderDto dto)
        {
            var existingOrder = await _context.Orders.FindAsync(id);
            if (existingOrder == null)
                return NotFound();

            var product = await _productApi.GetProductByIdAsync(dto.ProductId);
            if (product == null)
                return BadRequest("Product not found.");

            existingOrder.ProductId = dto.ProductId;
            existingOrder.Quantity = dto.Quantity;
            existingOrder.TotalPrice = product.Price * dto.Quantity;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ---------------------------------------------------------
        // DELETE ORDER
        // ---------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
