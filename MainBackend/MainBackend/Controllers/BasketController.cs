using MainBackend.Data;
using MainBackend.ViewModels.BasketVMs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MainBackend.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kukini oxuyuruq
            string? basketJson = HttpContext.Request.Cookies["basket"];

            if (string.IsNullOrEmpty(basketJson))
            {
                return View(new List<BasketDetailVM>()); // Sebet bosdursa bos siyahi gonder
            }

            List<BasketVM> basket = JsonConvert.DeserializeObject<List<BasketVM>>(basketJson);

            // Kukideki butun ID-leri bir siyahiya yigiriq
            List<int> ids = basket.Select(x => x.Id).ToList();

            // Bazaya tek bir sorgu gonderirik (Performans buradadir!)
            var products = await _context.Products
                .Include(m => m.Category)       // Kateqoriya melumatini qos
                .Include(m => m.ProductImages)      // Sekilleri qos
                .Where(m => ids.Contains(m.Id) && !m.IsDeleted)     // Siyahidaki ID-leri tap
                .ToListAsync();

            // Bazadan gelen real melumatlarla kukideki sayi birlesdiririk
            List<BasketDetailVM> basketDetailVMs = new();

            foreach (var product in products)
            {
                // Kukide bu mehsuldan nece dene oldugunu tapiriq
                var basketItem = basket.FirstOrDefault(b => b.Id == product.Id);

                basketDetailVMs.Add(new BasketDetailVM
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Count = basketItem.Count,   // Kukiden gelir
                    TotalPrice = product.Price * basketItem.Count,
                    Category = product.Category.Name,
                    Image = product.ProductImages.FirstOrDefault(m => m.IsMain)?.Image
                });
            }

            return View(basketDetailVMs);       // Artiq detalli siyahini gonderirik
        }

        [HttpPost]
        public async Task<IActionResult> Add(int? id)
        {
            if (id is null) return BadRequest();

            var isExist = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);

            if (isExist == null) return NotFound();

            List<BasketVM> basketVMs;

            if (HttpContext.Request.Cookies["basket"] != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(HttpContext.Request.Cookies["basket"]);
            }
            else
            {
                basketVMs = new List<BasketVM>();
            }

            var existProduct = basketVMs.FirstOrDefault(m => m.Id == id);

            if (existProduct != null)
            {
                existProduct.Count++;
            }
            else
            {
                basketVMs.Add(new BasketVM()
                {
                    Id = (int)id,
                    Count = 1,
                    Price = isExist.Price
                });
            }

            HttpContext.Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketVMs));

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            // Kukini oxuyuruq

            string basketJson = HttpContext.Request.Cookies["basket"];

            if (string.IsNullOrEmpty(basketJson))
            {
                return NotFound();
            }

            List<BasketVM> basket = JsonConvert.DeserializeObject<List<BasketVM>>(basketJson);

            // Siyahidan hemin ID-li mehsulu tapiriq ve silirik

            var productToRemove = basket.FirstOrDefault(m => m.Id == id);

            if(productToRemove != null)
            {
                basket.Remove(productToRemove);
            }

            // Yenilenmis siyahini yeniden Cookie-ye yaziriq

            HttpContext.Response.Cookies.Append("basket", JsonConvert.SerializeObject(basket));

            // Səbət səhifəsinə geri qayidiriq (Səhifə avtomatik yenilənəcək)
            return RedirectToAction(nameof(Index));
        }
    }
}
