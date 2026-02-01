using System.ComponentModel.DataAnnotations;

namespace MainBackend.ViewModels.AccountVMs
{
    public class ForgotPasswordVM
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
