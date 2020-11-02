// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
