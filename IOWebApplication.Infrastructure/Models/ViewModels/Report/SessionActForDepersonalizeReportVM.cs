// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Актове за обезличаване
    /// </summary>
    public class SessionActForDepersonalizeReportVM
    {
        [Display(Name = "Точен вид дело")]
        public string CaseTypeName { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        public int CaseId { get; set; }

        public int SessionActId { get; set; }

        [Display(Name = "Вид")]
        public string SessionActTypeName { get; set; }

        [Display(Name = "Номер")]
        public string SessionActNumber { get; set; }

        [Display(Name = "Дата")]
        public DateTime SessionActDate { get; set; }

        [Display(Name = "Заседание")]
        public string SessionTypeName { get; set; }
    }

    /// <summary>
    /// Филтър Актове за обезличаване
    /// </summary>
    public class SessionActForDepersonalizeFilterReportVM
    {
        [Display(Name = "От дата на постановяване")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата на постановяване")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Финализиращ акт")]
        public string IsFinalAct { get; set; }
    }
}
