using Microsoft.AspNetCore.Mvc;
using Orders_MVC_Project.Models.ViewModels;
using Orders_MVC_Project.Services;

namespace Orders_MVC_Project.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public OrdersController(IOrderService orderService, IProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetOrdersAsync();
            return View(orders);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            // Get available products directly from ProductService
            var products = await _productService.GetProductsAsync();
            ViewBag.AvailableProducts = products;
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
           
                try
                {
                    // First verify the product exists in ProductService
                    var product = await _productService.GetProductByIdAsync(request.ProductId);
                    if (product == null)
                    {
                        ModelState.AddModelError("ProductId", "Selected product does not exist");
                    }
                    else
                    {
                        await _orderService.CreateOrderAsync(request);
                        TempData["SuccessMessage"] = "Order created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating order: {ex.Message}");
                }
            

            // Reload products if validation fails
            var products = await _productService.GetProductsAsync();
            ViewBag.AvailableProducts = products;
            return View(request);
        }

        // GET: Orders/SyncProducts
        [HttpGet]
        public async Task<IActionResult> SyncProducts()
        {
            try
            {
                // Test ProductService connection first
                var productServiceConnected = await _productService.TestConnectionAsync();
                if (!productServiceConnected)
                {
                    TempData["ErrorMessage"] = "Cannot connect to ProductService. Make sure it's running on https://localhost:7259";
                    return RedirectToAction(nameof(Create));
                }

                // Sync products from ProductService to OrderService
                var success = await _orderService.SyncProductsAsync();
                if (success)
                {
                    TempData["SuccessMessage"] = "Products synced successfully from ProductService to OrderService!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to sync products from ProductService to OrderService.";
                }
                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error syncing products: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: Orders/TestConnections
        [HttpGet]
        public async Task<IActionResult> TestConnections()
        {
            try
            {
                var productServiceStatus = await _productService.TestConnectionAsync();
                var products = await _productService.GetProductsAsync();

                ViewBag.ProductServiceStatus = productServiceStatus ? "Connected" : "Disconnected";
                ViewBag.ProductsCount = products?.Count ?? 0;
                ViewBag.Products = products;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // Keep other methods (Edit, Delete, Stats, etc.) the same
        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var updateRequest = new UpdateOrderRequest
            {
                ProductId = order.ProductId,
                Quantity = order.Quantity
            };

            ViewBag.OrderId = id;
            return View(updateRequest);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateOrderRequest request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.UpdateOrderAsync(id, request);
                    TempData["SuccessMessage"] = "Order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating order: {ex.Message}");
                }
            }

            ViewBag.OrderId = id;
            return View(request);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _orderService.DeleteOrderAsync(id);
                TempData["SuccessMessage"] = "Order deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting order: {ex.Message}");
                return View(await _orderService.GetOrderAsync(id));
            }
        }

        // GET: Orders/Stats
        public async Task<IActionResult> Stats()
        {
            var stats = await _orderService.GetOrderStatsAsync();
            return View(stats);
        }

        // Other methods remain the same...
        public IActionResult DateRange()
        {
            ViewBag.DefaultStartDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            ViewBag.DefaultEndDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                ModelState.AddModelError("", "Start date cannot be after end date");
                return View();
            }

            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View("DateRangeResults", orders);
        }

        public IActionResult Recent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Recent(int count)
        {
            if (count <= 0)
            {
                ModelState.AddModelError("", "Count must be greater than 0");
                return View();
            }

            var orders = await _orderService.GetRecentOrdersAsync(count);
            ViewBag.Count = count;
            return View("RecentResults", orders);
        }

        public IActionResult ProductOrders()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductOrders(int productId)
        {
            var orders = await _orderService.GetOrdersByProductAsync(productId);
            ViewBag.ProductId = productId;
            return View("ProductOrdersResults", orders);
        }

        public async Task<IActionResult> ProductPerformance()
        {
            var performance = await _orderService.GetProductPerformanceAsync();
            return View(performance);
        }
    }
}