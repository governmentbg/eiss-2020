// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class NotificationItemVM
    {
        public bool IsChecked { get; set; }
        public int PersonId { get; set; }
        public string PersonLabel { get; set; }
        public string PersonRole { get; set; }
        [Display(Name = "Връзки за лицето")]
        public int? LinkId { get; set; }
        public List<SelectListItem> LinkId_Ddl { get; set; }
        [Display(Name = "Адрес на получаване")]
        public long? AddressId { get; set; }
        public List<SelectListItem> AddressId_Ddl { get; set; }
        public bool IsLawUnit { get; set; }
        public int RowNumber { get; set; }

        [Display(Name = "Призовката ще се разнася от")]
        public int? ToCourtId { get; set; }
        [Display(Name = "Район")]
        public int? DeliveryAreaId { get; set; }

        [Display(Name = "Призовкар")]
        public int? LawUnitId { get; set; }

        [Display(Name = "Призовкар")]
        public string LawUnitName { get; set; }
    }
}
