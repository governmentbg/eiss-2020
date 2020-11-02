// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CasePersonFilterVM
    {
        [Display(Name = "Идентификатор на лице")]
        public string Uic { get; set; }


        [Display(Name = "Имена/Наименование на лице")]
        public string FullName { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }
    }
}
