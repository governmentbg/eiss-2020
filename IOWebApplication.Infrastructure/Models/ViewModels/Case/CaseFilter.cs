using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел за филтър на дела
    /// </summary>
    public class CaseFilter
    {
        /// <summary>
        /// Идентификатор на делото
        /// </summary>
        public int? CaseId { get; set; }

        /// <summary>
        /// Дата на образуване на дело
        /// </summary>
        public DateTime? RegDate { get; set; }

        [Display(Name = "Основен вид дело")]
        public string CaseGroupIds { get; set; }
        public string CaseGroupIds_text { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeIds { get; set; }
        public string CaseTypeIds_text { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Шифър")]
        public int CaseCodeId { get; set; }

        [Display(Name = "Шифър")]
        public string[] CaseCodeIds { get; set; }

        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        [Display(Name = "Година")]
        public int? CaseYear { get; set; }

        [Display(Name = "Номер на иницииращ документ")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Съдия-докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Индикатор")]
        public int CaseClassificationId { get; set; }

        [Display(Name = "Статус")]
        public int CaseStateId { get; set; }

        [Display(Name = "Състав")]
        public int? CourtDepartmentId { get; set; }

        [Display(Name = "Отделение")]
        public int? CourtDepartmentOtdelenieId { get; set; }
    }
}
