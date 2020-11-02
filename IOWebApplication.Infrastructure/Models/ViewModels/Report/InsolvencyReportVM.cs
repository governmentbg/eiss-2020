// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class InsolvencyReportVM
    {
        public int CaseId { get; set; }
        public int CaseGroupId { get; set; }

        [Display(Name = "№ и година на делото")]
        public string CaseNumber { get; set; }

        [Display(Name = "Докладчик (състав)")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "Фирма, седалище и адрес на призоваване на длъжника")]
        public string DebtorName { get; set; }

        [Display(Name = "№ по ред на действ.")]
        public int NumberAction { get; set; }

        [Display(Name = "Дата на постъпване на книжата")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DocumentDate { get; set; }

        public int? SessionId { get; set; }
        public string SessionData { get; set; }

        public string SessionActData { get; set; }
        public long? DocumentId { get; set; }
        public int? DocumentActId { get; set; }
        public List<long> SessionDocumentIds { get; set; }

        [Display(Name = "Дата на извършване на действ.")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime SessionDate { get; set; }

        [Display(Name = "Вид на действието Молба, жалба, частна жалба")]
        public string DocumentTypeName { get; set; }

        [Display(Name = "Участник: длъжник, кредитор, комитет на кредиторите, събрание на кредиторите, синдик")]
        public string PersonNames { get; set; }

        [Display(Name = "Актове на съда по несъстоятелността: решения, определения, разпореждания")]
        public string Acts { get; set; }

        [Display(Name = "Актове на Апелативен съд")]
        public string ActsApeal { get; set; }

        [Display(Name = "Актове на Върховен касационен съд")]
        public string ActsVKS { get; set; }

        public DateTime DateOrder1 { get; set; }
        public DateTime DateOrder2 { get; set; }
    }

    public class InsolvencyFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Номер на дело")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string CaseNumber { get; set; }

        [Display(Name = "Година")]
        public int? CaseYear { get; set; }
    }

    public class InsolvencyCaseReportVM
    {
        public int Id { get; set; }

        public int CaseGroupId { get; set; }

        public string RegNumber { get; set; }

        public string DebtorName { get; set; }

        public long DocumentId { get; set; }
    }
}
