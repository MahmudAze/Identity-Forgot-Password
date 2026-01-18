using System.ComponentModel.DataAnnotations;

namespace MainBackend.Areas.Admin.ViewModels.ProductVMs
{
    public class CreateProductVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public IFormFile MainImage { get; set; }

        [Required]
        public IEnumerable<int> TagIds { get; set; }

        [Required]
        public IEnumerable<IFormFile> AdditionalImages { get; set; }
    }
}
