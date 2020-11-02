// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class AddressFilterVM
    {
        [Display(Name = "Вид адрес")]
        public int AddressTypeId { get; set; }

        [Display(Name = "Държава")]
        public string CountryCode { get; set; }

        [Display(Name = "Населено място")]
        public string CityCode { get; set; }
    }
}
