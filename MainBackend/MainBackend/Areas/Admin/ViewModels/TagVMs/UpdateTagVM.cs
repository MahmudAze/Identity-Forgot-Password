using System.ComponentModel.DataAnnotations;

namespace MainBackend.Areas.Admin.ViewModels.TagVMs
{
    public class UpdateTagVM
    {
        [Required]
        public string Name { get; set; }
    }
}
