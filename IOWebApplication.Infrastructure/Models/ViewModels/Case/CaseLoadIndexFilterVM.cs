using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLoadIndexFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Съдия")]
        public int? LawUnitId { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Шифър")]
        public int CaseCodeId { get; set; }

        [Display(Name = "Номер на дело")]
        public string RegNumber { get; set; }

        [Display(Name = "Състав")]
        public int? CourtDepartmentId { get; set; }

        [Display(Name = "Отделение")]
        public int? CourtDepartmentOtdelenieId { get; set; }

        [Display(Name = "Вид заседание")]
        public int SessionTypeId { get; set; }

        [Display(Name = "Резултат от заседание")]
        public int SessionResultId { get; set; }

        [Display(Name = "Вид акт")]
        public int ActTypeId { get; set; }

        [Display(Name = "Дейност")]
        public int JudgeLoadActivityId { get; set; }
    }
}
