using System.ComponentModel.DataAnnotations;

namespace MainBackend.Areas.Admin.ViewModels.TagVMs
{
    public class CreateTagVM
    {
        [Required]
        public string Name { get; set; }
    }
}
