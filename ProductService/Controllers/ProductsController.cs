using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ✅ Needed for EF queries
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductsController(ProductDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            // Return all products
            return Ok(_context.products.ToList());
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProduct(int id)
        {
            // Search within DbSet
           var product = _context.products.FirstOrDefault(x=>x.Id == id); 
            

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
               return BadRequest(ModelState);   
            }

            var products= await _context.products.AnyAsync(p=>p.Id == product.Id);
            if (products)
            {
                return Conflict($"Product with ID {product.Id} already exists");
            }

            _context.products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);

        }

        [HttpDelete("{id}")]

        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var products = await _context.products.FindAsync(id);
            if(products == null)
            {
                return NotFound();
            }
            _context.products.Remove(products); 
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("search")]

        public async Task<ActionResult<Product>> SearchProducts([FromQuery] string Name)
        {
            if (String.IsNullOrWhiteSpace(Name))
            {
                return BadRequest();
            }

            var products =  _context.products.Where(p => p.Name.Contains(Name));
            if (products == null)
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpGet("price-range")]

        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByPriceRange([FromQuery] double minPrice, [FromQuery] double maxPrice)
        {

            if (minPrice > maxPrice || minPrice < 0)
            {
               return BadRequest("Minimum Price Cannot be Greater than Maximum Price");
            }

        var products= await _context.products.
                Where(p=>p.Price>=minPrice&&p.Price<=maxPrice).
                OrderBy(p=>p.Price).ToListAsync();  
            return Ok(products);
        }

        [HttpGet("stats")]

        public async  Task<ActionResult<Product>> GetProductStats()
        {
            var stats = new
            {
                TotalProducts = await _context.products.CountAsync(),
                AveragePrice = await _context.products.AverageAsync(p => p.Price),
                MaxPrice=await _context.products.MaxAsync(p => p.Price),
                MinPrice=await _context.products.MinAsync(p => p.Price),
            };
            return Ok(stats);
        }


        [HttpPut("{id}")]

        public async Task<ActionResult<Product>> UpdateProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.Id)
            {
                return NotFound();
            }

            var ExistingProducts = await _context.products.FindAsync(id);
            if (ExistingProducts == null)
            {
                return NotFound();

            }
            ExistingProducts.Name=product.Name;
            ExistingProducts.Price=product.Price;

            _context.products.Update(ExistingProducts);
            await _context.SaveChangesAsync();
            return NoContent();

        }


    }
}
