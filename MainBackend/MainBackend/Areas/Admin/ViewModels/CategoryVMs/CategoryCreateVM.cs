using System.ComponentModel.DataAnnotations;

namespace MainBackend.Areas.Admin.ViewModels.CategoryVMs
{
    public class CategoryCreateVM
    {
        [Required(ErrorMessage = "Kateqoriya adı boş ola bilməz")]
        [MaxLength(50, ErrorMessage = "Maksimum 50 simvol ola bilər")]
        public string Name { get; set; }
    }
}
