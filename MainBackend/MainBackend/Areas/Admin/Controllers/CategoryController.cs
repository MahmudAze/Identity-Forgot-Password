using MainBackend.Areas.Admin.ViewModels.CategoryVMs;
using MainBackend.Data;
using MainBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainBackend.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Category> categories = await _context.Categories.OrderByDescending(c => c.Id).Where(c => !c.IsDeleted).ToListAsync();

            IEnumerable<GetAllCategoryVM> getAllCategoryVMs = categories.Select(c => new GetAllCategoryVM()
            {
                Id = c.Id,
                Name = c.Name
            });


            return View(getAllCategoryVMs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryCreateVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateVM categoryVM)
        {
            if (!ModelState.IsValid) return View(categoryVM);

            // Daha temiz ve suretli yoxlanis
            bool isExist = await _context.Categories
                .AnyAsync(c => c.Name.Trim().ToLower() == categoryVM.Name.Trim().ToLower());

            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu adda kateqoriya artıq mövcuddur!");
                return View(categoryVM);
            }

            // VM-dən Entity-yə keçid (Mapping)
            Category category = new()
            {
                Name = categoryVM.Name.Trim()
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            Category? category = await _context.Categories
                .FirstOrDefaultAsync(m => !m.IsDeleted && m.Id == id);

            if (category == null) return NotFound();

            DetailCategoryVM detailCategoryVM = new()
            {
                Id = category.Id,
                Name = category.Name
            };

            return View(detailCategoryVM);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            Category? category = await _context.Categories
                .FirstOrDefaultAsync(m =>  m.Id == id && !m.IsDeleted);

            if (category == null) return NotFound();

            UpdateCategoryVM updateCategoryVM = new()
            {
                Name = category.Name
            };

            return View(updateCategoryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int Id, UpdateCategoryVM request)
        {
            if (!ModelState.IsValid) return View();

            Category category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == Id && !m.IsDeleted);

            if (category == null) return NotFound();

            bool isExist = await _context.Categories.AnyAsync(m => m.Name.Trim().ToLower() == request.Name.Trim().ToLower());

            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu adda kateqoriya mövcuddur.");
                return View();
            }

            category.Name = request.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            Category? category = await _context.Categories.FirstOrDefaultAsync(m =>m.Id == id && !m.IsDeleted);

            if (category == null) return NotFound();

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
