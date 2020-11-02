// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppDataDocumentVM
    {
        [Display(Name = "Номер документ")]
        public string Number { get; set; }
        public DateTime Date { get; set; }
    }
}
