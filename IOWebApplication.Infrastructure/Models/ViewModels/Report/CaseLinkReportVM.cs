// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseLinkReportVM
    {
        public int CaseId { get; set; }

        [Display(Name = "Дело/документ")]
        public string CaseData { get; set; }

        [Display(Name = "Документ")]
        public string OutDocument { get; set; }

        [Display(Name = "Вид на съдебния акт")]
        public string ActData { get; set; }

        [Display(Name = "Резултат")]
        public string SessionResult { get; set; }

        [Display(Name = "Дата на съдеб. акт")]
        public string ActDate { get; set; }

        [Display(Name = "Адресант")]
        public string PersonName { get; set; }

        [Display(Name = "Вид служебно дело")]
        public string CaseInstitutionType { get; set; }

        [Display(Name = "Вид съдебно дело")]
        public string CaseLinkType { get; set; }

        [Display(Name = "Номер служебно дело/година")]
        public string CaseLinkNumber { get; set; }
    }

    public class CaseLinkFilterReportVM
    {
        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "От номер дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? FromCaseNumber { get; set; }

        [Display(Name = "До номер дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Въведете стойност по-голяма от 0")]
        public int? ToCaseNumber { get; set; }

        [Display(Name = "От дата на образуване на делото")]
        public DateTime? DateFromCase { get; set; }

        [Display(Name = "До дата  на образуване на делото")]
        public DateTime? DateToCase { get; set; }

        [Display(Name = "Вид движение")]
        public int CaseMigrationTypeId { get; set; }

        [Display(Name = "Вид служебно дело")]
        public int InstitutionCaseTypeId { get; set; }

        [Display(Name = "Съд")]
        public int FromCourtId { get; set; }

        [Display(Name = "Тип институция")]
        public int InstitutionTypeId { get; set; }

        [Display(Name = "Институция")]
        public int InstitutionId { get; set; }

        [Display(Name = "Служебно дело номер")]
        public string CaseLinkNumber { get; set; }

        [Display(Name = "Служебно дело година")]
        public int? CaseLinkYear { get; set; }

        [Display(Name = "Основен вид на документа")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид на документа")]
        public int DocumentTypeId { get; set; }

        [Display(Name = "От дата на свършване")]
        public DateTime? DateFromAct { get; set; }

        [Display(Name = "До дата на свършване")]
        public DateTime? DateToAct { get; set; }
    }
}
