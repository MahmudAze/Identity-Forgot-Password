using System.ComponentModel.DataAnnotations;

namespace MainBackend.ViewModels.AccountVMs
{
    public class LoginVM
    {
        [Required]
        public string UsernameOrEmail { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
