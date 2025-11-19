using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Client;
using OrderService.Data;
using OrderService.Models;
using OrderService.Models.DTOs;

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
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    Quantity = o.Quantity,
                    TotalPrice = o.TotalPrice,
                    OrderDate = o.OrderDate,
                    Product = o.Product != null ? new ProductDto
                    {
                        Id = o.Product.Id,
                        Name = o.Product.Name,
                        Price = o.Product.Price
                    } : null
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Product)
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    ProductId = o.ProductId,
                    Quantity = o.Quantity,
                    TotalPrice = o.TotalPrice,
                    OrderDate = o.OrderDate,
                    Product = o.Product != null ? new ProductDto
                    {
                        Id = o.Product.Id,
                        Name = o.Product.Name,
                        Price = o.Product.Price
                    } : null
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than 0");
                }

                var localProduct = await _context.Products.FindAsync(request.ProductId);
                if (localProduct == null)
                {
                    return NotFound($"Product with ID {request.ProductId} not found");
                }

                var totalPrice = request.Quantity * localProduct.Price;
                var order = new Order
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    TotalPrice = totalPrice,
                    OrderDate = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Reload with product data and return as DTO
                var createdOrder = await _context.Orders
                    .Include(o => o.Product)
                    .Where(o => o.Id == order.Id)
                    .Select(o => new OrderDto
                    {
                        Id = o.Id,
                        ProductId = o.ProductId,
                        Quantity = o.Quantity,
                        TotalPrice = o.TotalPrice,
                        OrderDate = o.OrderDate,
                        Product = o.Product != null ? new ProductDto
                        {
                            Id = o.Product.Id,
                            Name = o.Product.Name,
                            Price = o.Product.Price
                        } : null
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { error = "An error occurred while creating the order" });
            }
        }

        // Update other methods similarly...
        [HttpGet("available-products")]
        public async Task<ActionResult> GetAvailableProducts()
        {
            try
            {
                var localProducts = await _context.Products
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price
                    })
                    .ToListAsync();

                return Ok(localProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available products");
                return StatusCode(500, "Error getting available products");
            }
        }

        [HttpPost("sync-products")]
        public async Task<ActionResult> SyncProductsFromProductService()
        {
            try
            {
                var productsFromService = await _productApiClient.GetProductsAsync();

                if (productsFromService == null || !productsFromService.Any())
                {
                    return Ok(new { message = "No products found in ProductService to sync" });
                }

                var existingProductIds = await _context.Products.Select(p => p.Id).ToListAsync();

                var newProducts = new List<Product>();
                var updatedProducts = new List<Product>();

                foreach (var externalProduct in productsFromService)
                {
                    var existingProduct = await _context.Products.FindAsync(externalProduct.Id);

                    if (existingProduct == null)
                    {
                        newProducts.Add(new Product
                        {
                            Id = externalProduct.Id,
                            Name = externalProduct.Name,
                            Price = externalProduct.Price
                        });
                    }
                    else
                    {
                        if (existingProduct.Name != externalProduct.Name || existingProduct.Price != externalProduct.Price)
                        {
                            existingProduct.Name = externalProduct.Name;
                            existingProduct.Price = externalProduct.Price;
                            updatedProducts.Add(existingProduct);
                        }
                    }
                }

                if (newProducts.Any())
                {
                    _context.Products.AddRange(newProducts);
                }

                if (updatedProducts.Any())
                {
                    _context.Products.UpdateRange(updatedProducts);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Products synced successfully",
                    newProductsCount = newProducts.Count,
                    updatedProductsCount = updatedProducts.Count,
                    totalProductsInOrderService = await _context.Products.CountAsync()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing products from ProductService");
                return StatusCode(500, new { error = "Error syncing products", details = ex.Message });
            }
        }

        // Add other methods (Update, Delete, Stats, etc.) with DTOs...
    }
}