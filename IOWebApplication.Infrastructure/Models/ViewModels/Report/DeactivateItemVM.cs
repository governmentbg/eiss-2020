// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class DeactivateItemVM
    {
        public int SourceType { get; set; }
        public long SourceId { get; set; }

        public string SourceInfo { get; set; }
        public DateTime? SourceDate { get; set; }
        public string DeactivateUserName { get; set; }
        public DateTime DeactivateDate { get; set; }
        public string DeactivateDescription { get; set; }

        public static string RegisterName
        {
            get
            {
                return "Регистър на премахнатите обекти";
            }
        }
    }

    public class DeactivateItemFilterVM
    {
        [Display(Name = "Вид обект")]
        public int SourceType { get; set; }
        [Display(Name = "Описание")]
        public string SourceInfo { get; set; }
        [Display(Name = "От дата")]
        public DateTime? SourceDateFrom { get; set; }
        [Display(Name = "До дата")]
        public DateTime? SourceDateTo { get; set; }

        [Display(Name = "Деактивирано от дата")]
        public DateTime? DeactivateDateFrom { get; set; }
        [Display(Name = "Деактивирано до дата")]
        public DateTime? DeactivateDateTo { get; set; }
        [Display(Name = "Деактивирано от")]
        public string DeactivateUserName { get; set; }
    }
}