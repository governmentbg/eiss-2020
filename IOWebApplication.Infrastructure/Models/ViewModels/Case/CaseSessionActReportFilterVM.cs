using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActReportFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Шифър")]
        public int CaseCodeId { get; set; }

        [Display(Name = "Тип акт")]
        public int ActTypeId { get; set; }

        [Display(Name = "Основен вид докумет")]
        public int DocumentGroupId { get; set; }

        [Display(Name = "Точен вид документ")]
        public int DocumentTypeId { get; set; }

        [Display(Name = "Съдия докладчик")]
        public int JudgeReporterId { get; set; }

        [Display(Name = "Резултат/степен на уважаване на иска")]
        public int? ActComplainResultId { get; set; }

        [Display(Name = "Влизане в сила от")]
        public DateTime? ActInforcedDateFrom { get; set; }

        [Display(Name = "Влизане в сила до")]
        public DateTime? ActInforcedDateTo { get; set; }

        [Display(Name = "Вид производство")]
        public int? ProcessPriorityId { get; set; }

        [Display(Name = "Резултат от заседание")]
        public int? SessionResultId { get; set; }

        [Display(Name = "Статус на акт")]
        public int? ActStateId { get; set; }
    }
}
