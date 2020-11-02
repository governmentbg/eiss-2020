// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseSessionPublicReportVM
    {
        [Display(Name = "№ по ред")]
        public int Index { get; set; }

        [Display(Name = "Дата на заседанието")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime SessionDate { get; set; }

        [Display(Name = "№ и дата на образуване на делото")]
        public string CaseData { get; set; }

        [Display(Name = "Състав на съда, Председател, Членове")]
        public string CaseLawUnits { get; set; }

        public string CourtDepartmentName { get; set; }

        [Display(Name = "Докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Прокурор")]
        public string ProsecutorName { get; set; }

        [Display(Name = "Секретар")]
        public string SecretaryName { get; set; }

        [Display(Name = "Жалбоподател")]
        public string LeftSide { get; set; }

        [Display(Name = "Ответник")]
        public string RightSide { get; set; }

        [Display(Name = "Жалбоподател")]
        public string LeftSideGrajdansko { get; set; }

        [Display(Name = "Ответник")]
        public string RightSideGrajdansko { get; set; }

        [Display(Name = "Резултат от заседанието")]
        public string SessionResult { get; set; }

        [Display(Name = "№ на съдебния акт")]
        public string SessionAct { get; set; }

        [Display(Name = "Решение")]
        public string SessionActDecision { get; set; }

        [Display(Name = "Дата, за която е отложено; причини за отлагането")]
        public string SessionAdjourn { get; set; }

        [Display(Name = "Дата на предаване на делото в канцеларията")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateCaseOffice { get; set; }

        [Display(Name = "Подпис на служителя")]
        public string Signature { get; set; }

        [Display(Name = "Характер")]
        public string CaseTypeCode { get; set; }

        [Display(Name = "По кой член, алинея, точка от закона е обвинението")]
        public string CaseCodeName { get; set; }

        public int CaseLifecycleMonths { get; set; }

        [Display(Name = "Страни по делото")]
        public string CasePersons { get; set; }

        public string ActResultName { get; set; }

        public bool IsNewNumberCase { get; set; }
        public bool IsOldNumberCase { get; set; }
        public bool IsStop { get; set; }

        public int CaseTypeId { get; set; }

        public string LawUnitJury { get; set; }

        public DateTime? ActInforcedDate { get; set; }

        public int CasePersonCount { get; set; }
        public int CasePersonUnder18Count { get; set; }

        public string RecidiveGeneral { get; set; }
        public string RecidiveDanger { get; set; }
        public string RecidiveSpecial { get; set; }
    }

    public class CaseSessionPublicFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Съдебен състав")]
        public int DepartmentId { get; set; }

        [Display(Name = "Инстанция")]
        public int InstanceId { get; set; }

    }
}
