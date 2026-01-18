using MainBackend.Models;

namespace MainBackend.Areas.Admin.ViewModels.ProductVMs
{
    public class DetailProductVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public IEnumerable<ProductImage> ProductImages { get; set; }
    }
}
