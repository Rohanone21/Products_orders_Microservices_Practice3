using Microsoft.AspNetCore.Mvc;
using Orders_New_MVC_Microservice.Models;
using Orders_New_MVC_Microservice.Models.ViewModels;
using Orders_New_MVC_Microservice.Services;
using System.Text;

namespace Orders_New_MVC_Microservice.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public OrdersController(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        // ------------------------- Dashboard -------------------------
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var products = await _productService.GetAllProductsAsync();

            // Filter by date
            if (startDate.HasValue && endDate.HasValue)
            {
                orders = orders
                    .Where(o => o.OrderDate.Date >= startDate.Value.Date &&
                                o.OrderDate.Date <= endDate.Value.Date)
                    .ToList();
            }

            // Convert to OrderWithName
            var orderWithNames = orders.Select(o => new OrderWithName
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                ProductName = products.FirstOrDefault(p => p.Id == o.ProductId)?.Name ?? "Unknown",
                Quantity = o.Quantity,
                OrderDate = o.OrderDate
            }).ToList();

            var viewModel = new DashboardViewModel
            {
                Products = products,
                Orders = orders.Select(o => new OrderWithName
                {
                    OrderId = o.OrderId,
                    CustomerName = o.CustomerName,
                    ProductId = o.ProductId,
                    ProductName = products.FirstOrDefault(p => p.Id == o.ProductId)?.Name,
                    ProductPrice = (decimal)(products.FirstOrDefault(p => p.Id == o.ProductId)?.Price ?? 0),
                    Quantity = o.Quantity,
                    OrderDate = o.OrderDate,
                    IsPaid = o.IsPaid         // ✅ FIXED
                }).ToList()
            };

            return View(viewModel);
        }


        // ----------------------- Create Order ------------------------
        public async Task<IActionResult> Create(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(string customerName, int productId, int quantity)
        {
            var newOrder = new Order
            {
                CustomerName = customerName,
                ProductId = productId,
                Quantity = quantity,
                OrderDate = DateTime.Now
            };

            var status = await _orderService.CreateOrderAsync(newOrder);

            if (status)
                return RedirectToAction("Index");

            return View("Error");
        }


        // ------------------------- Statistics -------------------------
        public async Task<IActionResult> Stats()
        {
            var stats = await _orderService.GetStatsAsync();
            return View(stats);
        }

        // ---------------------- Latest Orders -------------------------
        //public async Task<IActionResult> Latest(int count = 5)
        //{
        //    var latest = await _orderService.GetLatestOrdersAsync(count);
        //    return View(latest);
        //}

        public async Task<IActionResult> Latest(int count = 5)
        {
            var latestOrders = await _orderService.GetLatestOrdersAsync(count);
            var products = await _productService.GetAllProductsAsync();

            // Convert Order → OrderWithName
            var viewModel = latestOrders.Select(o => new OrderWithName
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                ProductId = o.ProductId,
                ProductName = products.FirstOrDefault(p => p.Id == o.ProductId)?.Name ?? "Unknown",
                Quantity = o.Quantity,
                OrderDate = o.OrderDate,
                IsPaid = o.IsPaid
            }).ToList();

            return View(viewModel);
        }


        // ---------------- Product Performance ------------------------
        public async Task<IActionResult> ProductPerformance()
        {
            var data = await _orderService.GetProductPerformanceAsync();
            return View(data);
        }

        // ---------------- Most Ordered Product ------------------------
        public async Task<IActionResult> MostOrderedProduct()
        {
            var data = await _orderService.GetMostOrderedProductAsync();
            return View(data);
        }

        // ---------------- Product Demand ------------------------------
        public async Task<IActionResult> ProductDemand()
        {
            var demand = await _orderService.GetProductDemandAsync();
            return View(demand);
        }

        // ---------------- Orders by Date Range ------------------------
        public async Task<IActionResult> OrdersByDateRange(DateTime start, DateTime end)
        {
            var orders = await _orderService.GetOrdersByDateRangeAsync(start, end);
            return View(orders);
        }

        // ------ Product Performance by Date Range ----------------------
        public async Task<IActionResult> ProductPerformanceDateRange(DateTime start, DateTime end)
        {
            var perf = await _orderService.GetProductPerformanceDateRangeAsync(start, end);
            return View(perf);
        }
        //public async Task<IActionResult> AllOrders()
        //{
        //    var orders = await _orderService.GetAllOrdersAsync();
        //    return View(orders);
        //}
        public async Task<IActionResult> AllOrders()
        {
            // Fetch data
            var orders = await _orderService.GetAllOrdersAsync();
            var products = await _productService.GetAllProductsAsync();

            // Map Orders → OrderWithName
            var orderWithNames = orders.Select(o => new OrderWithName
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                ProductId = o.ProductId,
                ProductName = products.FirstOrDefault(p => p.Id == o.ProductId)?.Name ?? "Unknown",
                ProductPrice = (decimal)(products.FirstOrDefault(p => p.Id == o.ProductId)?.Price ?? 0),
                Quantity = o.Quantity,
                OrderDate = o.OrderDate,
                IsPaid = o.IsPaid
            }).ToList();

            // Send to view
            return View(orderWithNames);
        }


        public async Task<IActionResult> OrdersTable()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var products = await _productService.GetAllProductsAsync();

            var orderList = orders.Select(o => new OrderWithName
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                ProductId = o.ProductId,
                ProductName = products.FirstOrDefault(p => p.Id == o.ProductId)?.Name,
                ProductPrice = (decimal)(products.FirstOrDefault(p => p.Id == o.ProductId)?.Price ?? 0),
                Quantity = o.Quantity,
                TotalPrice = (decimal)((products.FirstOrDefault(p => p.Id == o.ProductId)?.Price ?? 0) * o.Quantity),
                OrderDate = o.OrderDate,

                // ✅ THE MOST IMPORTANT FIX
                IsPaid = o.IsPaid
            }).ToList();

            return View(orderList);
        }


        [HttpPut("MarkPaid/{id}")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var result = await _orderService.MarkOrderAsPaidAsync(id);

            if (!result)
                return NotFound();

            return Ok(new { success = true });
        }



        [HttpPost]
        public IActionResult SendOrderEmail([FromBody] EmailRequest req)
        {
            EmailHelper helper = new EmailHelper();

            bool status = helper.Send(req.ToEmail, req.Subject, req.Message);

            return status ? Ok() : BadRequest("Email failed");
        }

        public class EmailRequest
        {
            public string ToEmail { get; set; }
            public string Subject { get; set; }
            public string Message { get; set; }
        }








    }
}
