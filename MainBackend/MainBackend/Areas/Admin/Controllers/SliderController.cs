using MainBackend.Areas.Admin.ViewModels.SliderVMs;
using MainBackend.Data;
using MainBackend.Helpers;
using MainBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainBackend.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SliderController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Slider> sliders = await _context.Sliders.Where(m => !m.IsDeleted).ToListAsync();

            IEnumerable<GetAllSliderVM> getAllSliderVMs = sliders.Select(s => new GetAllSliderVM()
            {
                Id = s.Id,
                Image = s.Image
            });

            return View(getAllSliderVMs);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSliderVM request)
        {
            if (!ModelState.IsValid) return View();

            if (!request.Photo.CheckFileType("image/"))
            {
                ModelState.AddModelError("Photo", "Şəkil tipində olmalıdır!");
                return View();
            }

            if (request.Photo.CheckFileSize(2000))
            {
                ModelState.AddModelError("Photo", "Şəkilin ölcüsü max 2MB ola bilər!");
                return View();
            }

            string fileName = request.Photo.GenerateFileName();

            string path = _env.WebRootPath.GetFilePath("img", fileName);

            request.Photo.SaveFile(path);

            Slider newSlider = new()
            {
                Image = fileName
            };

            await _context.AddAsync(newSlider);
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id is null) return BadRequest();

            Slider? slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (slider == null) return NotFound();

            UpdateSliderVM updateSliderVM = new()
            {
                Image = slider.Image
            };

            return View(updateSliderVM);
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Slider? slider = await _context.Sliders.FirstOrDefaultAsync(m => !m.IsDeleted && m.Id == id);

            if (slider == null) return NotFound();

            DetailSliderVM detailSliderVM = new()
            {
                Image = slider.Image
            };

            return View(detailSliderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, UpdateSliderVM request)
        {
            if(id is null) return BadRequest();

            Slider? slider = await _context.Sliders.FirstOrDefaultAsync(s =>  !s.IsDeleted && s.Id == id);

            if(slider == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View();
            }

            if(request.Photo != null)
            {
                if (!request.Photo.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Photo", "Şəkil tipində olmalıdır!");
                    request.Image = slider.Image;
                    return View(request);
                }

                if (request.Photo.CheckFileSize(2000))
                {
                    ModelState.AddModelError("Photo", "Şəkilin ölcüsü max 2MB ola bilər!");
                    request.Image = slider.Image;
                    return View(request);
                }

                string oldPath = _env.WebRootPath.GetFilePath("img", slider.Image);
                if (System.IO.File.Exists(oldPath)) // Fayl həqiqətən varmı?
                {
                    System.IO.File.Delete(oldPath); // Varsa sil
                }

                string fileName = request.Photo.GenerateFileName();

                string newPath = _env.WebRootPath.GetFilePath("img", fileName);

                request.Photo.SaveFile(newPath);

                slider.Image = fileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Slider? slider = await _context.Sliders.FirstOrDefaultAsync(s =>  s.Id == id && !s.IsDeleted);

            if (slider == null) return NotFound();

            // 1. Faylın tam yolunu alırıq (Helper-dən istifadə etmisən deyə mən də elə yazıram)
            string path = _env.WebRootPath.GetFilePath("img", slider.Image);

            if (System.IO.File.Exists(path)) // Fayl həqiqətən varmı?
            {
                System.IO.File.Delete(path); // Varsa sil
            }

            // 3. Verilənlər bazasından (DB) silirik
            _context.Sliders.Remove(slider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
