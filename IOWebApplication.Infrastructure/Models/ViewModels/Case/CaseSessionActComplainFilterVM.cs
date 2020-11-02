// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActComplainFilterVM
    {
        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Индекс")]
        public int ActComplainIndexId { get; set; }

        [Display(Name = "Резултат")]
        public int ActResultId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "От дата на регистриране на жалбата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата на регистриране на жалбата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "От дата на връщане от горна инстанция")]
        public DateTime? DateFromActReturn { get; set; }

        [Display(Name = "До дата на връщане от горна инстанция")]
        public DateTime? DateToActReturn { get; set; }

        [Display(Name = "От дата на изх. писмо за обжалване")]
        public DateTime? DateFromSendDocument { get; set; }

        [Display(Name = "До дата на изх писмо за обжалване")]
        public DateTime? DateToSendDocument { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Номер на акт")]
        public string ActRegNumber { get; set; }

        [Display(Name = "От номер на дело")]
        public int CaseRegNumFrom { get; set; }

        [Display(Name = "До номер на дело")]
        public int CaseRegNumTo { get; set; }
    }
}
