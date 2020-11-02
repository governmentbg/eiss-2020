// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkTaskFilterVM
    {
        [Display(Name = "Дата от")]
        public DateTime? DateFrom { get; set; }
        [Display(Name = "Дата до")]
        public DateTime? DateTo { get; set; }
        [Display(Name = "Насочена")]
        public int UserMode { get; set; }
        [Display(Name = "Насрещна страна")]
        public string UserId { get; set; }
        [Display(Name = "Вид задача")]
        public int? TaskTypeId { get; set; }
        [Display(Name = "Статус")]
        public int? TaskStateId { get; set; }
        [Display(Name = "Описание")]
        public string SourceDescription { get; set; }

        [Display(Name = "Пояснение")]
        public string ParentDescription { get; set; }

        [Display(Name = "Възложена от")]
        public string CreatedBy { get; set; }

        [Display(Name = "Възложена на")]
        public string AssignedTo { get; set; }
    }
}
