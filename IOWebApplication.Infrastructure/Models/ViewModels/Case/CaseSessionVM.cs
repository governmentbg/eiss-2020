using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionVM
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int CaseId { get; set; }

        [Display(Name = "Вид заседаниe")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int SessionTypeId { get; set; }

        [Display(Name = "Зала")]
        public int? CourtHallId { get; set; }

        [Display(Name = "Статус на заседание")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int SessionStateId { get; set; }
        [Display(Name = "Номер на дело")]
        public string CaseName { get; set; }
        [Display(Name = "Вид дело")]
        public string CaseTypeLabel { get; set; }
        [Display(Name = "Вид заседание")]
        public string SessionTypeLabel { get; set; }
        [Display(Name = "Зала")]
        public string CourtHallName { get; set; }
        [Display(Name = "Статус")]
        public string SessionStateLabel { get; set; }
        [Display(Name = "Резултат")]
        public string SessionResultLabel { get; set; }

        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Display(Name = "Начало")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }
        public DateTime DateFromTime { get; set; }

        [Display(Name = "Дата")]
        public string DateFromDateString { get; set; }

        [Display(Name = "Час")]
        public string DateFromTimeString { get; set; }

        [Display(Name = "Край")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Прогнозна продължителност")]
        public int DateTo_Minutes { get; set; }

        public bool IsExpired { get; set; }

        public int CaseTypeId { get; set; }
        public int? NotificationListTypeId { get; set; }
        [Display(Name = "Съдия докладчик")]
        public string JudgeReporterLabel { get; set; }
        
        [Display(Name = "Състав")]
        public string JudgeCompositionLabel { get; set; }

        [Display(Name = "Тип акт")]
        public int? ActTypeId { get; set; }

        [Display(Name = "Вид")]
        public int? ActKindId { get; set; }

        [Display(Name = "Подлежи на обжалване")]
        public bool? ActCanAppeal { get; set; }

        public int? ActSaveId { get; set; }
        public string ActSaveType { get; set; }

        public int? CaseSessionOldId { get; set; }
        [Display(Name = "Номер")]
        public int? NumberRow { get; set; }

        public virtual List<CheckListVM> CaseLawUnitByCase { get; set; }
        public virtual List<CheckListVM> CaseSessions { get; set; }
    }
}
