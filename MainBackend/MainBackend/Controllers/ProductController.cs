using MainBackend.Data;
using MainBackend.Models;
using MainBackend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace MainBackend.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> products = await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted)
                //.Take(4)
                .ToListAsync();

            IEnumerable<Category> categories = await _context.Categories
                .Include(m => m.Products)
                .Where(m => !m.IsDeleted)
                .ToListAsync();

            ViewBag.MinPrice = await _context.Products.MinAsync(m => m.Price);
            ViewBag.MaxPrice = await _context.Products.MaxAsync(m => m.Price);

            HomeVM homeVM = new()
            {
                Products = products,
                Categories = categories
            };

            return View(homeVM);
        }
         

        public async Task<IActionResult> LoadMore(int skip)
        {
            IEnumerable<Product> products = await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted)
                .Skip(skip)
                .Take(4)
                .ToListAsync();

            IEnumerable<Category> categories = await _context.Categories
                .Include(m => m.Products)
                .Where(m =>  !m.IsDeleted)
                .ToListAsync();

            HomeVM homeVM = new()
            {
                Products = products,
                Categories = categories
            };

            return PartialView("_ProductPartial", homeVM);
        }

        public async Task<IActionResult> Search(string searchText)
        {
            IEnumerable<Product> products = await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted && m.Name.Contains(searchText) || searchText == null)
                .ToListAsync();

            HomeVM homeVM = new()
            {
                Products = products
            };

            return PartialView("_ProductPartial", homeVM);
        }

        public async Task<IActionResult> FilterCategory(int categoryId)
        {
            IEnumerable<Product> products = await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted && m.Category.Id == categoryId)
                .ToListAsync();

            HomeVM homeVM = new()
            {
                Products = products
            };

            return PartialView("_ProductPartial", homeVM);
        }

        public async Task<IActionResult> Sorting(string sortingType)
        {
            IEnumerable<Product> products = await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted)
                .ToListAsync();

            switch(sortingType)
            {
                case "lowToHigh":
                    products = products.OrderBy(m => m.Price);
                    break;

                case "highToLow":
                    products = products.OrderByDescending(m => m.Price);
                    break;

                case "AZ":
                    products = products.OrderBy(m => m.Name);
                    break;

                case "ZA":
                    products = products.OrderByDescending(m => m.Name);
                    break;

                default:
                    break;
            }

            HomeVM homeVM = new()
            {
                Products = products
            };

            return PartialView("_ProductPartial", homeVM);
        }
    }
}
