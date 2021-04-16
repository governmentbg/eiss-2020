using IOWebApplication.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Account
{
    public class UserProfileRegisterVM : UserProfileVM
    {
        [Display(Name = "Парола *")]
        [IORequired]
        public string Password { get; set; }

        [Display(Name = "Повторете паролата *")]
        public string Password2 { get; set; }
    }
}
