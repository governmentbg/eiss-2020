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

        [Display(Name = "Основен вид дело")]
        public string CaseGroupIds { get; set; }
        public string CaseGroupIds_text { get; set; }

        [Display(Name = "Точен вид дело")]
        public string CaseTypeIds { get; set; }
        public string CaseTypeIds_text { get; set; }

        [Display(Name = "Състав")]
        public int CourtDepartmentId { get; set; }

        [Display(Name = "Отделение")]
        public int CourtDepartmentOtdelenieId { get; set; }

        [Display(Name = "Съдия-докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Вид акт")]
        public string ActTypeIds { get; set; }
        public string ActTypeIds_text { get; set; }
    }
}
