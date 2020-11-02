// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class DocumentOutReportVM
    {
        [Display(Name = "Изходящ номер")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Дата")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DocumentDate { get; set; }

        [Display(Name = "Описание на изпращания документ; адресат")]
        public string Description { get; set; }

        [Display(Name = "Начин на изпращане")]
        public string DeliveryGroupName { get; set; }

        public int DocumentNumberValue { get; set; }
    }

    public class DocumentOutFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeId { get; set; }

        public string CaseTypeIds { get; set; }

        [Display(Name = "От номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? FromNumber { get; set; }

        [Display(Name = "До номер")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? ToNumber { get; set; }
    }
}
