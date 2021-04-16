using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Account
{
    public class PasswordResetVM
    {
        public string UserId { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }

        [Display(Name = "Нова парола *")]
        [Required(ErrorMessage = "Въведете '{0}'.")]
        public string Password { get; set; }

        [Display(Name = "Повторете новата парола *")]
        public string Password2 { get; set; }
    }
}
