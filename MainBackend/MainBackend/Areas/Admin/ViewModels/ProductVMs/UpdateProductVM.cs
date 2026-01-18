using MainBackend.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;

namespace MainBackend.Areas.Admin.ViewModels.ProductVMs
{
    public class UpdateProductVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IEnumerable<int>? TagIds { get; set; }

        public IEnumerable<ProductTag>? Tags { get; set; }

        public string? CategoryName { get; set; }

        public string? MainPhoto { get; set; }

        public IEnumerable<string>? AdditionalPhotos { get; set; }
        public IFormFile? MainImage { get; set; }
        public IEnumerable<IFormFile>? AdditionalImages { get; set; }

        public IEnumerable<ProductImage>? ProductImages { get; set; }

    }
}
