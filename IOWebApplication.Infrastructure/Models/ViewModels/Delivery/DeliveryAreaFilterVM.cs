// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using IOWebApplication.Infrastructure.Constants;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryAreaFilterVM
    {
        public int CourtId { get; set; }

        // -1 Всички
        // 0 Анулирани
        // 1 Активни
        [Display(Name = "Активни/Анулирани")]
        public int ExpiredType { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "От дата анулиране")]
        public DateTime? DateExpiredFrom { get; set; }

        [Display(Name = "До дата анулиране")]
        public DateTime? DateExpiredTo { get; set; }
    }
}
