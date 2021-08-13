// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Identity
{
    public class UserFilterVM
    {
        public string UserId { get; set; }

        [Display(Name = "Имена")]
        public string FullName { get; set; }
        [Display(Name = "Електронна поща")]
        public string Email { get; set; }

        public void UpdateNullables()
        {
            FullName = FullName.EmptyToNull();
            Email = Email.EmptyToNull();
        }
    }
}
