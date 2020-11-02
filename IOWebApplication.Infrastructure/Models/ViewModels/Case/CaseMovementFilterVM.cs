// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseMovementFilterVM
    {
        [Display(Name = "Изпрати/приел")]
        public string UserId { get; set; }

        [Display(Name = "Дело номер")]
        public string CaseRegNum { get; set; }
    }
}
