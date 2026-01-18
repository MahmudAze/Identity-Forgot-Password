using System.ComponentModel.DataAnnotations;

namespace MainBackend.Areas.Admin.ViewModels.SliderVMs
{
    public class CreateSliderVM
    {
        [Required]
        public IFormFile Photo { get; set; }
    }
}
