using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActComplainResultEditVM
    {
        public int Id { get; set; }
        public int? ComplainCourtId { get; set; }
        
        [Display(Name = "Дело")]
        public int? ComplainCaseId { get; set; }

        public int CaseSessionActComplainId { get; set; }
        public int CourtId { get; set; }
        public int CaseId { get; set; }
        
        [Display(Name = "Акт")]
        public int? CaseSessionActId { get; set; }

        [Display(Name = "Резултат от обжалване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int? ActResultId { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Дата на отразяване на резултат")]
        public DateTime? DateResult { get; set; }

        [Display(Name = "Дело от друга система")]
        public bool CaseOtherSystem { get; set; }

        [Display(Name = "Номер дело")]
        public string CaseRegNumberOtherSystem { get; set; }

        [Display(Name = "Година дело")]
        public int? CaseYearOtherSystem { get; set; }

        [Display(Name = "Код на дело")]
        public string CaseShortNumberOtherSystem { get; set; }

        [Display(Name = "Акт")]
        public string CaseSessionActOtherSystem { get; set; }

        [Display(Name = "Начална дата на интервал")]
        public DateTime? DateFromLifeCycle { get; set; }

        [Display(Name = "Стартирай нов интервал по делото")]
        public bool IsStartNewLifecycle { get; set; }

        public virtual List<CheckListVM> CaseSessionActComplains { get; set; }
    }
}
