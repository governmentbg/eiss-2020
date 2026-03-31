// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class NotificationDocItemVM
    {
        public bool IsChecked { get; set; }
        public long PersonId { get; set; }
        public string PersonLabel { get; set; }
        [Display(Name = "Връзки за лицето")]
        public int? LinkId { get; set; }
        public List<SelectListItem> LinkId_Ddl { get; set; }
        [Display(Name = "Адрес на получаване")]
        public long? AddressId { get; set; }
        public List<SelectListItem> AddressId_Ddl { get; set; }
    }
}
