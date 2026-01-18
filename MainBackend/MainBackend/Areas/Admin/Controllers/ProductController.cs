using MainBackend.Areas.Admin.ViewModels.CategoryVMs;
using MainBackend.Areas.Admin.ViewModels.ProductVMs;
using MainBackend.Data;
using MainBackend.Helpers;
using MainBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MainBackend.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1, int take = 5)
        {
            // 1) Get distinct product ids from ProductTags
            var productIds = await _context.ProductTags
                .Where(p => !p.IsDeleted)
                .Select(p => p.ProductId)
                .Distinct()
                .OrderByDescending(id => id)
                .Skip((page * take) - take)
                .Take(take)
                .ToListAsync();

            // 2) Load products and necessary navigations for those ids
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(p => p.Tag)
                .OrderByDescending(p => p.Id)
                .ToListAsync();


            // 3) Project to VM in memory
            var getAllProductVMs = products
                .Select(p => new GetAllProductVM
                {
                    Id = p.Id,
                    MainImage = p.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category?.Name,
                    Tags = p.ProductTags.Select(pt => pt.Tag.Name).ToList()
                })
                .ToList();

            int pageCount = await GetPageCount(take);

            Paginate<GetAllProductVM> paginate = new(getAllProductVMs, page, pageCount);

            return View(paginate);
        }

        public async Task<int> GetPageCount(int take)
        {
            int count = await _context.Products.Where(m => !m.IsDeleted).CountAsync();

            return (int)Math.Ceiling((decimal)count / take);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await GetAllCategories();
            ViewBag.Tags = await GetAllTags();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetAllCategories();
                ViewBag.Tags = await GetAllTags();
                return View();
            }

            if (!request.MainImage.CheckFileType("image/"))
            {
                ViewBag.Categories = await GetAllCategories();
                ViewBag.Tags = await GetAllTags();

                ModelState.AddModelError("MainImage", "Şəkil tipində olmalıdır.");
                return View();
            }

            if (request.MainImage.CheckFileSize(2000))
            {
                ViewBag.Categories = await GetAllCategories();
                ViewBag.Tags = await GetAllTags();

                ModelState.AddModelError("MainImage", "Şəkil maks 2 MB olmalıdır.");
                return View();
            }

            foreach (var item in request.AdditionalImages)
            {
                if (!item.CheckFileType("image/"))
                {
                    ViewBag.Categories = await GetAllCategories();
                    ViewBag.Tags = await GetAllTags();

                    ModelState.AddModelError("AdditionalImages", "Şəkil tipində olmalıdır.");
                    return View();
                }

                if (item.CheckFileSize(2000))
                {
                    ViewBag.Categories = await GetAllCategories();
                    ViewBag.Tags = await GetAllTags();

                    ModelState.AddModelError("AdditionalImages", "Şəkil maks 2 MB olmalıdır.");
                    return View();
                }
            }

            bool isExist = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && !c.IsDeleted);

            if (!isExist)
            {
                ViewBag.Categories = await GetAllCategories();
                ViewBag.Tags = await GetAllTags();

                ModelState.AddModelError("CategoryId", "Kateqoriya tapılmadı.");
                return View();
            }

            List<ProductImage> productImages = new();

            string mainFileName = request.MainImage.GenerateFileName();
            string mainImagePath = _env.WebRootPath.GetFilePath("img", mainFileName);
            request.MainImage.SaveFile(mainImagePath);

            ProductImage newMainImage = new()
            {
                Image = mainFileName,
                IsMain = true
            };

            productImages.Add(newMainImage);

            foreach (var item in request.AdditionalImages)
            {
                string additionalFileName = item.GenerateFileName();
                string additionalImagePath = _env.WebRootPath.GetFilePath("img", additionalFileName);
                item.SaveFile(additionalImagePath);

                ProductImage newAdditionalImage = new()
                {
                    Image = additionalFileName
                };

                productImages.Add(newAdditionalImage);
            }

            Product newProduct = new()
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                ProductImages = productImages
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();

            // Gələn TagId-lərin bazada olub-olmadığını yoxlayırıq
            var existingTagIds = await _context.Tags
                .Where(t => request.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            // Yalnız bazada tapılan teqlər üçün əlaqə yaradırıq
            foreach (var tagId in existingTagIds)
            {
                ProductTag productTag = new()
                {
                    ProductId = newProduct.Id,
                    TagId = tagId
                };
                await _context.ProductTags.AddAsync(productTag);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null) return BadRequest();

            // 1) Get product ids from ProductTags
            var productId = await _context.ProductTags
                .Where(p => !p.IsDeleted && p.ProductId == id)
                .Select(p => p.ProductId)
                .Distinct()
                .FirstOrDefaultAsync();

            // 2) Load products and necessary navigations for those ids
            var newProduct = await _context.Products
                .Where(p => p.Id == productId && !p.IsDeleted)
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(p => p.Tag)
                .FirstOrDefaultAsync();

            if (newProduct == null) return NotFound();

            // tagIds seçilən məhsullardarın içindən tad İD-ləri yığır
            var tagIds = newProduct.ProductTags.Select(pt => pt.TagId).ToList();

            // seçilən tag İD-lər ilə taqları siyahıya yığır
            var tags = await _context.Tags
                .Where(t => !t.IsDeleted && !tagIds.Contains(t.Id))
                .ToListAsync();

            ViewBag.Categories = await GetAllCategories();
            ViewBag.Tags = _context.Tags.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            });

            UpdateProductVM updateProductVM = new()
            {
                Name = newProduct.Name,
                Description = newProduct.Description,
                Price = newProduct.Price,
                CategoryId = newProduct.CategoryId.Value,
                MainPhoto = newProduct.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image,
                AdditionalPhotos = newProduct.ProductImages.Where(pi => !pi.IsMain).Select(pi => pi.Image).ToList(),
                ProductImages = newProduct.ProductImages.Where(pi => !pi.IsMain).ToList(),
                Tags = newProduct.ProductTags
            };

            return View(updateProductVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM request)
        {
            if (id == null) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags)
                .FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

            if (product == null) return NotFound();

            bool isExist = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && !c.IsDeleted);

            if (!isExist)
            {
                ViewBag.Categories = await GetAllCategories();
                ViewBag.Tags = await GetAllTags();

                request.MainPhoto = product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image;
                request.AdditionalPhotos = product.ProductImages.Where(pi => !pi.IsMain).Select(pi => pi.Image).ToList();

                ModelState.AddModelError("CategoryId", "Category tapilmadi!");
                return View(request);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await GetAllCategories();
                ViewBag.Tags = await GetAllTags();

                request.MainPhoto = product.ProductImages.FirstOrDefault(pi => pi.IsMain)?.Image;
                request.AdditionalPhotos = product.ProductImages.Where(pi => !pi.IsMain).Select(pi => pi.Image).ToList();

                return View(request);
            }

            // Taqlarin yenilenmesi
            if (request.TagIds != null)
            {
                // 1. Köhnə taqları bazadan silirik
                _context.ProductTags.RemoveRange(product.ProductTags);

                // 2. Seçilmiş yeni taqları əlavə edirik
                foreach (var tagId in request.TagIds)
                {
                    product.ProductTags.Add(new ProductTag
                    {
                        TagId = tagId,
                        ProductId = product.Id
                    });
                }
            }

            if (request.MainImage != null)
            {
                // Köhnə şəkli qovluqdan silirik
                var mainImg = product.ProductImages.FirstOrDefault(pi => pi.IsMain);
                if (mainImg != null)
                {
                    string oldMainPath = Path.Combine(_env.WebRootPath, "img", mainImg.Image);
                    if (System.IO.File.Exists(oldMainPath)) System.IO.File.Delete(oldMainPath);

                    // Yeni şəkli saxlayırıq
                    string mainFileName = request.MainImage.GenerateFileName();
                    string mainPath = _env.WebRootPath.GetFilePath("img", mainFileName);
                    request.MainImage.SaveFile(mainPath);

                    mainImg.Image = mainFileName; // Adı yeniləyirik
                }
            }

            // 2. Additional Images hissəsi
            if (request.AdditionalImages != null && (request.AdditionalImages.Count() > 0))
            {
                // Köhnə əlavə şəkilləri həm qovluqdan, həm DB-dən silmək istəyirsinizsə:
                var oldAdditionals = product.ProductImages.Where(pi => !pi.IsMain).ToList();
                foreach (var item in oldAdditionals)
                {
                    string path = Path.Combine(_env.WebRootPath, "img", item.Image);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

                    _context.ProductImages.Remove(item); // Köhnələri bazadan silirik
                }

                // Yeni şəkilləri əlavə edirik
                foreach (var item in request.AdditionalImages)
                {
                    string fileName = item.GenerateFileName();
                    string path = _env.WebRootPath.GetFilePath("img", fileName);
                    item.SaveFile(path);

                    product.ProductImages.Add(new ProductImage
                    {
                        Image = fileName,
                        IsMain = false,
                        ProductId = product.Id
                    });
                }
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Product? product = await _context.Products
                .Include(m => m.ProductImages)
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            DetailProductVM detailProductVM = new()
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryName = product.Category.Name,
                ProductImages = product.ProductImages
            };

            return View(detailProductVM);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            Product? product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTag(int? id, int? productId)
        {
            if (id == null || productId == null) return BadRequest();

            ProductTag? productTag = await _context.ProductTags
                .FirstOrDefaultAsync(m => m.TagId == id && m.ProductId == productId && !m.IsDeleted);

            if (productTag == null) return NotFound();

            _context.Remove(productTag);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int? id)
        {
            var image = await _context.ProductImages.FindAsync(id);

            if (image == null) return NotFound();

            // şəkli qovluqdan silirik
            string path = Path.Combine(_env.WebRootPath, "img", image.Image);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            // Bazadan silirik
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<SelectList> GetAllCategories()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return new SelectList(categories, "Id", "Name");
        }

        public async Task<SelectList> GetAllTags()
        {
            var tags = await _context.Tags
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return new SelectList(tags, "Id", "Name");
        }
    }
}
