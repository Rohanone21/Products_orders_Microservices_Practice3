using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Client;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IProductApiClient _productApiClient;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderDbContext context, IProductApiClient productApiClient, ILogger<OrdersController> logger)
        {
            _context = context;
            _productApiClient = productApiClient;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetOrders()
        {
            return Ok(_context.Orders.ToList());
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new order for product ID: {ProductId}, Quantity: {Quantity}",
                    request.ProductId, request.Quantity);

                var product = await _productApiClient.GetProductByIdAsync(request.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", request.ProductId);
                    return NotFound($"Product with ID {request.ProductId} not found");
                }

                var totalPrice = request.Quantity * product.Price;
                var order = new Order
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    TotalPrice = totalPrice,
                    OrderDate = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);

                return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for product ID: {ProductId}", request.ProductId);
                return StatusCode(500, "An error occurred while creating the order");
            }
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult<Order>> DeleteOrder(int id)
        {
            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(orders);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingOrder = await _context.Orders.FindAsync(id);
                if (existingOrder == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for update", id);
                    return NotFound();
                }

                // If product ID changed, validate the new product
                if (existingOrder.ProductId != request.ProductId)
                {
                    var product = await _productApiClient.GetProductByIdAsync(request.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Product with ID {ProductId} not found for order update", request.ProductId);
                        return NotFound($"Product with ID {request.ProductId} not found");
                    }

                    existingOrder.ProductId = request.ProductId;
                    existingOrder.TotalPrice = request.Quantity * product.Price;
                }
                else if (existingOrder.Quantity != request.Quantity)
                {
                    // If only quantity changed, recalculate total price
                    var product = await _productApiClient.GetProductByIdAsync(existingOrder.ProductId);
                    existingOrder.TotalPrice = request.Quantity * product.Price;
                }

                existingOrder.Quantity = request.Quantity;
                existingOrder.OrderDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order with ID {OrderId} updated successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order with ID: {OrderId}", id);
                return StatusCode(500, "An error occurred while updating the order");
            }
        }
        [HttpGet("stats")]

        public async Task<ActionResult<IEnumerable<Order>>> GetOrderStats()
        {
            var orders = new
            {
                TotalOrders = await _context.Orders.CountAsync(),
                TotalRevenue = await _context.Orders.SumAsync(o => o.TotalPrice),
                AverageOrderValue = await _context.Orders.AverageAsync(o => o.TotalPrice),
                MaxOrderValue = await _context.Orders.MaxAsync(o => o.TotalPrice),
                MinOrderValue = await _context.Orders.MinAsync(o => o.TotalPrice),
                TodayOrders = await _context.Orders.Where(o=>o.OrderDate.Date== DateTime.UtcNow).ToListAsync(),
            };
            return Ok(orders);
        }


        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be after end date");
            }

            var orders = await _context.Orders.
                Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).
                OrderBy(o => o.OrderDate).ToListAsync();
            return Ok(orders);

        }


        public class CreateOrderRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public class UpdateOrderRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}