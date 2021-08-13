using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawyerHelpEditVM
    {
        public int Id { get; set; }
        public int? CourtId { get; set; }
        public int CaseId { get; set; }

        [Display(Name = "Основание за изпращане")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}")]
        public int LawyerHelpBaseId { get; set; }

        [Display(Name = "Вид правна помощ")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}")]
        public int LawyerHelpTypeId { get; set; }

        [Display(Name = "Акт за допускане на ПП")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}")]
        public int CaseSessionActId { get; set; }

        [Display(Name = "Противоречиви интереси ")]
        public bool HasInterestConflict { get; set; }

        [Display(Name = "Служебен защитник на предходна инстанция")]
        public string PrevDefenderName { get; set; }

        [Display(Name = "Допълнителна информация относно искането")]
        public string Description { get; set; }

        [Display(Name = "Да се явят на заседание")]
        public int? CaseSessionToGoId { get; set; }

        [Display(Name = "Съдебен акт за назначаване")]
        public int? ActAppointmentId { get; set; }

        [Display(Name = "Основание за назначаване")]
        public int? LawyerHelpBasisAppointmentId { get; set; }

        public virtual List<CheckListVM> CaseLawyerHelpOtherLawyers { get; set; }
        public virtual List<CheckListVM> CaseLawyerHelpPeople { get; set; }
    }
}
