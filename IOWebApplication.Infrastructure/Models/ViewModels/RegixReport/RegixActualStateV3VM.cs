// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixActualStateV3VM
    {
        public RegixReportVM Report { get; set; }

        public RegixActualStateV3FilterVM ActualStateV3Filter { get; set; }

        public RegixActualStateV3ResponseVM ActualStateV3Response { get; set; }

        public RegixActualStateV3VM()
        {
            Report = new RegixReportVM();
            ActualStateV3Filter = new RegixActualStateV3FilterVM();
            ActualStateV3Response = new RegixActualStateV3ResponseVM();
        }
    }

    public class RegixActualStateV3FilterVM
    {
        [Display(Name = "ЕИК")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string UIC { get; set; }
    }

    public class RegixActualStateV3ResponseVM
    {
        [Display(Name = "Статус на партида:")]
        public string DeedStatus { get; set; }

        [Display(Name = "Наименование:")]
        public string CompanyName { get; set; }

        [Display(Name = "ЕИК:")]
        public string UIC { get; set; }

        [Display(Name = "Правна форма:")]
        public string LegalForm { get; set; }

        [Display(Name = "Номер на решението на съда, с което се учредява фирмата:")]
        public string CaseNo { get; set; }

        [Display(Name = "Година на решението на съда, с което се учредява фирмата:")]
        public string CaseYear { get; set; }

        [Display(Name = "Съд издал решението, с което се учредява фирмата:")]
        public string CourtNo { get; set; }

        [Display(Name = "Статус за ликвидация или несъстоятелност:")]
        public string LiquidationOrInsolvency { get; set; }

        public bool DataFound { get; set; }

        public List<RegixSubdeedVM> Subdeeds { get; set; }

        public RegixActualStateV3ResponseVM()
        {
            Subdeeds = new List<RegixSubdeedVM>();
        }
    }

    public class RegixSubdeedVM
    {
        [Display(Name = "Номер на раздел/клон:")]
        public string SubUIC { get; set; }

        [Display(Name = "Тип на раздела:")]
        public string SubUICType { get; set; }

        [Display(Name = "Статус на раздела:")]
        public string SubdeedStatus { get; set; }

        [Display(Name = "Име на раздел/клон:")]
        public string SubUICName { get; set; }

        public List<RegixRecordVM> Records { get; set; }

        public RegixSubdeedVM()
        {
            Records = new List<RegixRecordVM>();
        }
    }

    public class RegixRecordVM
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
