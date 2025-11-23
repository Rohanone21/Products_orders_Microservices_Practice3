using Microsoft.AspNetCore.Mvc;
using Orders_MVC_Project.DTO_s;
using Orders_MVC_Project.Models.ViewModels;
using Orders_MVC_Project.Services;
using Orders_MVC_Project.ViewModels;

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
            return View(new OrderListViewModel { Orders = orders! });
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();

            var products = await _productService.GetProductsAsync();
            var product = products?.FirstOrDefault(p => p.Id == order.ProductId);

            return View(new OrderDetailsViewModel
            {
                Order = order,
                Product = product!
            });
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            var products = await _productService.GetProductsAsync();
            return View(new CreateOrderViewModel
            {
                Order = new CreateOrderRequest(),
                Products = products!
            });
        }

        // POST: Orders/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderViewModel vm)
        {
          
                vm.Products = await _productService.GetProductsAsync();
                return View(vm);
       

            bool success = await _orderService.CreateOrderAsync(vm.Order);

            if (success)
                return RedirectToAction(nameof(Index));

            vm.Products = await _productService.GetProductsAsync();
            return View(vm);
        }
    }
}
