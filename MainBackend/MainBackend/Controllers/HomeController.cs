using System.Diagnostics;
using MainBackend.Data;
using MainBackend.Models;
using MainBackend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainBackend.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Slider> sliders = await _context.Sliders.Where(m => !m.IsDeleted).ToListAsync();

            SliderDetail sliderDetails = await _context.SliderDetails.FirstOrDefaultAsync(m => !m.IsDeleted);

            IEnumerable<Product> products = await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted)
                .Take(4)
                .ToListAsync();

            IEnumerable<Category> categories = await _context.Categories.Where(m => !m.IsDeleted).ToListAsync();

            HomeVM homeVM = new()
            {
                Sliders = sliders,
                SliderDetails = sliderDetails,
                Products = products,
                Categories = categories
            };

            return View(homeVM);
        }

        public async Task<IActionResult> LoadMore(int skip)
        {
            IEnumerable<Product> products = await _context.Products
                .Include (m => m.ProductImages)
                .Include(m => m.Category)
                .Where(m => !m.IsDeleted)
                .Skip(skip)
                .Take(4)
                .ToListAsync();

            HomeVM homeVM = new()
            {
                Products = products
            };

            return PartialView("_ProductPartial", homeVM);
        }
    }
}
