// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class NotificationDocGroupVM
    {
        public long? DocumentId { get; set; }
        public long? DocumentResolutionId { get; set; }
        [Display(Name = "Вид известие")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете вид известие")]
        public int NotificationTypeId { get; set; }
        [Display(Name = "Бланка")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете бланка")]
        public int HtmlTemplateId { get; set; }
        [Display(Name = "Вид известяване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете вид известяване")]
        public int? NotificationDeliveryGroupId { get; set; }
        [Display(Name = "Дата на връчване")]
        public DateTime? DeliveryDate { get; set; }
        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете статус")]
        public int NotificationStateId { get; set; }
        public List<NotificationDocItemVM> NotificationItems { get; set; }
    }
}
