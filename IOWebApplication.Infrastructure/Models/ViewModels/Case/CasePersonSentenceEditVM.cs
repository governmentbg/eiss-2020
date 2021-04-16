using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonSentenceEditVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int CourtId { get; set; }

        [Display(Name = "Постановена от")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int? DecreedCourtId { get; set; }

        [Display(Name = "Лице")]
        public int CasePersonId { get; set; }
        public string CasePersonName { get; set; }

        [Display(Name = "Акт")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int CaseSessionActId { get; set; }

        [Display(Name = "Резултат от съдебното производство")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете")]
        public int SentenceResultTypeId { get; set; }

        [Display(Name = "Описание за присъдата")]
        public string Description { get; set; }

        [Display(Name = "Активност на наказание")]
        public int? PunishmentActivityId { get; set; }

        [Display(Name = "Дата на активност на наказание")]
        public DateTime? PunishmentActivityDate { get; set; }

        [Display(Name = "Дата на влизане в сила на присъдата")]
        public DateTime? InforcedDate { get; set; }

        [Display(Name = "Дата на предаване за изпълнение")]
        public DateTime? ForInforcementDate { get; set; }

        [Display(Name = "Дата изпращане за изпълнение")]
        public DateTime? SentDate { get; set; }

        [Display(Name = "Срок за изпълнение")]
        public int? SentenceExecPeriodId { get; set; }

        [Display(Name = "Орган")]
        public int? InforcerInstitutionId { get; set; }

        [Display(Name = "Текст")]
        public string ExecDescription { get; set; }

        [Display(Name = "Предприети действия по изпълнение")]
        public DateTime? NotificationDate { get; set; }

        [Display(Name = "Дата на привеждане в изпълнение")]
        public DateTime? ExecDate { get; set; }

        [Display(Name = "Преписка")]
        public string InforcerDocumentNumber { get; set; }

        [Display(Name = "Зачита се от")]
        public DateTime? EffectiveDateFrom { get; set; }

        [Display(Name = "Място на изпълнение")]
        public int? ExecInstitutionId { get; set; }

        [Display(Name = "Номер и дата на указ за помилване")]
        public string AmnestyDocumentNumber { get; set; }

        [Display(Name = "Забележка")]
        public string ExecRemark { get; set; }

        [Display(Name = "Акт, който изменя/отменя присъда")]
        public int? ChangeCaseSessionActId { get; set; }

        [Display(Name = "Присъда, която е изменена/отменена")]
        public int? ChangedCasePersonSentenceId { get; set; }

        [Display(Name = "Активна")]
        public bool? IsActive { get; set; }

        [Display(Name = "Входящо писмо от орган за приведена присъда")]
        public string EnforceIncomingDocument { get; set; }

        [Display(Name = "Вх. номер/дата на Писмо за изпълнено наказание")]
        public string ExecIncomingDocument { get; set; }

        public virtual List<CheckListVM> LawBases { get; set; }
    }
}
