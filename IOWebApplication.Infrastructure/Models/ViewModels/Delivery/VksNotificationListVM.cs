// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class VksNotificationListVM
    {
        public int? CaseId { get; set; }
        public int CaseSessionId { get; set; }
        public string PaperEdition { get; set; }
        public bool CheckRow { get; set; }
        [Display(Name = "Вид")]
        public List<VksNotificationItemVM> VksNotificationItems { get; set; }
    }
}
