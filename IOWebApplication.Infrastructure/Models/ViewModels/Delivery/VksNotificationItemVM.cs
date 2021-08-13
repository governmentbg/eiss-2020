// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class VksNotificationItemVM
    {
        public int? NotificationListId { get; set; }
        public int RowNumber { get; set; }
        public string PersonName{ get; set; }
        public int? CasePersonId { get; set; }
        [Display(Name = "Връзки")]
        public int? CasePersonLinkId { get; set; }
        [Display(Name = "Адрес")]
        public long? NotificationAddressId { get; set; }
        [Display(Name = "Статус")]
        [Required(ErrorMessage = "Изберете статус")]
        public int? VksNotificationStateId { get; set; }
        [Display(Name = "Издание на държавен вестник")]
        public string PaperEdition { get; set; }
        public List<SelectListItem> CasePersonLinksDdl { get; set; }
        public List<SelectListItem> NotificationAddressesDdl { get; set; }
        public List<SelectListItem> VksNotificationStatesDdl { get; set; }
    }
}
