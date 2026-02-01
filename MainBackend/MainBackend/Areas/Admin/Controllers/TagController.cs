using MainBackend.Areas.Admin.ViewModels.CategoryVMs;
using MainBackend.Areas.Admin.ViewModels.TagVMs;
using MainBackend.Data;
using MainBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainBackend.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "SuperAdmin, Admin")]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Tag> tags = await _context.Tags.OrderByDescending(t => t.Id)
                .Where(t => !t.IsDeleted).ToListAsync();

            IEnumerable<GetAllTagVM> getAllTagVMs = tags.Select(t => new GetAllTagVM()
            {
                Id = t.Id,
                Name = t.Name
            });

            return View(getAllTagVMs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTagVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTagVM request)
        {
            if (!ModelState.IsValid) return View(request);

            // Daha temiz ve suretli yoxlanis
            bool isExist = await _context.Tags
                .AnyAsync(c => c.Name.Trim().ToLower() == request.Name.Trim().ToLower());

            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu adda tag artıq mövcuddur!");
                return View(request);
            }

            // VM-dən Entity-yə keçid (Mapping)
            Tag tag = new()
            {
                Name = request.Name.Trim()
            };

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            Tag? tag = await _context.Tags
                .FirstOrDefaultAsync(m => !m.IsDeleted && m.Id == id);

            if (tag == null) return NotFound();

            GetTagByIdVM detailTagVM = new()
            {
                Id = tag.Id,
                Name = tag.Name
            };

            return View(detailTagVM);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            Tag? tag = await _context.Tags
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (tag == null) return NotFound();

            UpdateTagVM updateTagVM = new()
            {
                Name = tag.Name
            };

            return View(updateTagVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int Id, UpdateTagVM request)
        {
            if (!ModelState.IsValid) return View();

            Tag tag = await _context.Tags.FirstOrDefaultAsync(m => m.Id == Id && !m.IsDeleted);

            if (tag == null) return NotFound();

            bool isExist = await _context.Tags.AnyAsync(m => m.Name.Trim().ToLower() == request.Name.Trim().ToLower());

            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu adda tag mövcuddur.");
                return View();
            }

            tag.Name = request.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            Tag? tag = await _context.Tags.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (tag == null) return NotFound();

            _context.Tags.Remove(tag);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
