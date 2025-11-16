using Microsoft.AspNetCore.Mvc;
using Products_MVC_Project.Models;
using Products_MVC_Project.Services;

namespace Products_MVC_Project.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetProductsAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productService.CreateProductAsync(product);
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                }
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productService.UpdateProductAsync(id, product);
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                }
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                TempData["SuccessMessage"] = "Product deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting product: {ex.Message}");
                return View(await _productService.GetProductAsync(id));
            }
        }

        // GET: Products/Search
        public IActionResult Search()
        {
            return View();
        }

        // POST: Products/Search
        [HttpPost]
        public async Task<IActionResult> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Please enter a search term");
                return View();
            }

            var products = await _productService.SearchProductsAsync(name);
            ViewBag.SearchTerm = name;
            return View("SearchResults", products);
        }

        // GET: Products/SearchResults
        public IActionResult SearchResults()
        {
            // This action is primarily accessed via POST from Search
            // but we provide a GET version for direct navigation
            return RedirectToAction(nameof(Search));
        }

        // GET: Products/PriceRange
        public IActionResult PriceRange()
        {
            return View();
        }

        // POST: Products/PriceRange
        [HttpPost]
        public async Task<IActionResult> PriceRange(double minPrice, double maxPrice)
        {
            if (minPrice > maxPrice || minPrice < 0)
            {
                ModelState.AddModelError("", "Minimum price cannot be greater than maximum price and must be non-negative");
                return View();
            }

            var products = await _productService.GetProductsByPriceRangeAsync(minPrice, maxPrice);
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            return View("PriceRangeResults", products);
        }

        // GET: Products/PriceRangeResults
        public IActionResult PriceRangeResults()
        {
            // This action is primarily accessed via POST from PriceRange
            // but we provide a GET version for direct navigation
            return RedirectToAction(nameof(PriceRange));
        }

        // GET: Products/Stats
        public async Task<IActionResult> Stats()
        {
            var stats = await _productService.GetProductStatsAsync();
            return View(stats);
        }

        // GET: Products/TopExpensive
        public IActionResult TopExpensive()
        {
            return View();
        }

        // POST: Products/TopExpensive
        [HttpPost]
        public async Task<IActionResult> TopExpensive(int count)
        {
            if (count <= 0)
            {
                ModelState.AddModelError("", "Count must be greater than 0");
                return View();
            }

            var products = await _productService.GetTopExpensiveProductsAsync(count);
            ViewBag.Count = count;
            return View("TopExpensiveResults", products);
        }

        // GET: Products/TopExpensiveResults
        public IActionResult TopExpensiveResults()
        {
            // This action is primarily accessed via POST from TopExpensive
            // but we provide a GET version for direct navigation
            return RedirectToAction(nameof(TopExpensive));
        }

        // GET: Products/NameStartsWith
        public IActionResult NameStartsWith()
        {
            return View();
        }

        // POST: Products/NameStartsWith
        [HttpPost]
        public async Task<IActionResult> NameStartsWith(string letter)
        {
            if (string.IsNullOrWhiteSpace(letter) || letter.Length != 1)
            {
                ModelState.AddModelError("", "Please enter a single letter");
                return View("NameStartsWith");
            }

            var products = await _productService.GetProductsByNameStartsWithAsync(letter);
            ViewBag.Letter = letter;
            return View("NameStartsWithResults", products);
        }

        // GET: Products/NameStartsWithResults
        public IActionResult NameStartsWithResults()
        {
            // This action is primarily accessed via POST from NameStartsWith
            // but we provide a GET version for direct navigation
            return RedirectToAction(nameof(NameStartsWith));
        }

        // GET: Products/NameStartsWith/{letter}
        [HttpGet("Products/NameStartsWith/{letter}")]
        public async Task<IActionResult> NameStartsWithDirect(string letter)
        {
            if (string.IsNullOrWhiteSpace(letter) || letter.Length != 1)
            {
                ModelState.AddModelError("", "Please enter a single letter");
                return View("NameStartsWith");
            }

            var products = await _productService.GetProductsByNameStartsWithAsync(letter);
            ViewBag.Letter = letter;
            return View("NameStartsWithResults", products);
        }
    }
}