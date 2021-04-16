﻿using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionFilterVM : DataTableFilterExportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Година")]
        public int? Year { get; set; }

        [Display(Name = "Вид заседание")]
        public int CaseSessionTypeId { get; set; }

        [Display(Name = "Зала")]
        public int HallId { get; set; }

        [Display(Name = "Секретар")]
        public string SecretaryUserId { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Основен вид дело")]
        public string CaseGroupIds { get; set; }
        public string CaseGroupIds_text { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeIds { get; set; }
        public string CaseTypeIds_text { get; set; }

        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        [Display(Name = "Резултат от заседание")]
        public int SessionResultId { get; set; }

        [Display(Name = "Статус")]
        public int SessionStateId { get; set; }

        [Display(Name = "Състав")]
        public int CourtDepartmentId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Отделение")]
        public int CourtDepartmentOtdelenieId { get; set; }
    }
}
