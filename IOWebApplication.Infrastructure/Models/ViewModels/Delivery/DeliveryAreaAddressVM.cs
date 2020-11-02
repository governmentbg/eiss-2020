// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryAreaAddressVM
    {
        public int Id { get; set; }
        
        [Display(Name = "Район")]
        public string AreaName { get; set; }

        [Display(Name = "Населено място")]
        public string City { get; set; }

        [Display(Name = "Улицa")]
        public string Street { get; set; }

        [Display(Name = "Квартал")]
        public string ResidentionArea { get; set; }

        [Display(Name = "Тип номера")]
        public string NumberType { get; set; }
 
        public int? NumberFrom { get; set; }

        public int? NumberTo { get; set; }

        public string BlockName { get; set; }

        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
    }
}
