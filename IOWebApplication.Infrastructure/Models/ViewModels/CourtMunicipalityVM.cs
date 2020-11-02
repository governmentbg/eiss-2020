// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CourtMunicipalityVM
    {
        public int Id { get; set; }
        [Display(Name = "Община")]
        public string Municipality { get; set; }
    }
}
