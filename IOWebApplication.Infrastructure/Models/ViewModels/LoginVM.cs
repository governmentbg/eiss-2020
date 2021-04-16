using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class LoginVM
    {
        [EmailAddress(ErrorMessage = "Полето {0} трябва да е валидна Електронна поща")]
        [Display(Name = "Електронна поща")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string Password { get; set; }

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public bool LoginWithPassword { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
