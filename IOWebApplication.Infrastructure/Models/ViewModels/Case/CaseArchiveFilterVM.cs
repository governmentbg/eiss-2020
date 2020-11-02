// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseArchiveFilterVM
    {
        [Display(Name = "От дата архив")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата архив")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер дело")]
        public string RegNumber { get; set; }

        [Display(Name = "Унищожени")]
        public bool IsDestroy { get; set; }
    }
}
