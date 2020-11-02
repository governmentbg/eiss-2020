// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class ZzdnReportVM
    {
        public long Id { get; set; }

        [Display(Name = "Пореден №")]
        public int Index { get; set; }

        [Display(Name = "Входящ документ номер и дата на постъпване")]
        public string DocumentData { get; set; }

        [Display(Name = "Дело")]
        public string CaseNumber { get; set; }

        [Display(Name = "Страни")]
        public string CasePersons { get; set; }
    }

    public class ZzdnFilterReportVM
    {
        [Display(Name = "От номер дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? FromCaseNumber { get; set; }

        [Display(Name = "До номер дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? ToCaseNumber { get; set; }

        [Display(Name = "Година на дело")]
        public int? CaseYear { get; set; }

        [Display(Name = "От дата документ")]
        public DateTime? FromDateDocument { get; set; }

        [Display(Name = "До дата документ")]
        public DateTime? ToDateDocument { get; set; }


    }
}
