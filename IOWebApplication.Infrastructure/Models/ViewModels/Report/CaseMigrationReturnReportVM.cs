// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class CaseMigrationReturnReportVM
    {
        [Display(Name = "№ по ред")]
        public int Index { get; set; }

        public string OldLinkNumber { get; set; }
        public string MigrationLinkNumber { get; set; }

        [Display(Name = "№ и година на първоинстанционно дело и от кой първоинстанционен съд постъпва")]
        public string InitialCaseData 
        {
            get
            {
                return string.IsNullOrEmpty(OldLinkNumber) == false ? OldLinkNumber : MigrationLinkNumber;
            }
        }

        [Display(Name = "Вид дело/документ №/ година на текущия съд")]
        public string CaseData { get; set; }

        [Display(Name = "Вид №/дата на съдебния акт")]
        public string ActReturnData { get; set; }

        [Display(Name = "Основание за връщане")]
        public string ActReturnDescription { get; set; }

        [Display(Name = "Съдия-докладчик")]
        public string JudgeReporterName { get; set; }

        [Display(Name = "№ на писмото и дата на връщане на делото/документа в първоинстанционни съд")]
        public string OutDocumentData { get; set; }

        [Display(Name = "Име и подпис на служител")]
        public string UserName { get; set; }

        [Display(Name = "Дата на постъпване на първоинстанционно дело и номер на новообразуваното дело")]
        public string NewCaseData { get; set; }
    }

    public class CaseMigrationReturnFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Съдия-докладчик")]
        public int JudgeReporterName { get; set; }

        [Display(Name = "Първоинстанционен съд")]
        public int InitialCourtId { get; set; }

        [Display(Name = "Съдебен състав")]
        public int DepartmentId { get; set; }
    }
}
