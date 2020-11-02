// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    ///  Свършени дела за период
    /// </summary>
    public class CaseFinishListReportVM
    {
        [Display(Name = "Точен вид дело")]
        public string CaseTypeName { get; set; }

        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Предмет и шифър")]
        public string CaseCodeName { get; set; }

        [Display(Name = "Резултат/Степен на уважаване на иска")]
        public string ActComplainResultName { get; set; }

        [Display(Name = "Резултат от заседанието")]
        public string SessionResultName { get; set; }

        [Display(Name = "Причина за прекратяване")]
        public string SessionResultStopBaseName { get; set; }

        [Display(Name = "Дата на приключване на делото")]
        public string CaseDateFinish { get; set; }

        [Display(Name = "Продължителност")]
        public int CaseLifecycleMonths { get; set; }

        [Display(Name = "Първоинстанционен съд")]
        public string InitialCourtName { get; set; }
    }

    /// <summary>
    /// Филтър Свършени дела за период
    /// </summary>
    public class CaseFinishListFilterReportVM
    {
        [Display(Name = "От дата на приключване")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата на приключване")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Шифър")]
        public int CaseCodeId { get; set; }

        [Display(Name = "Резултат/Степен на уважаване на иска")]
        public int ActComplainResultId { get; set; }

        [Display(Name = "Резултат от заседанието")]
        public int SessionResultId { get; set; }

        [Display(Name = "Първоинстанционен съд")]
        public int InitialCourtId { get; set; }
    }
}
