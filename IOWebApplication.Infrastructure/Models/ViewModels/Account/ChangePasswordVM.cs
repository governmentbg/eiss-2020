using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Account
{
    public class ChangePasswordVM : PasswordResetVM
    {
        [Display(Name = "Съществуваща парола *")]
        [Required(ErrorMessage = "Въведете '{0}'.")]
        public string OldPassword { get; set; }
    }
}
