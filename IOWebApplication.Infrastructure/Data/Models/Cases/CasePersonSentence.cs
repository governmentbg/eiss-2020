using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Присъда по лице в дело
    /// </summary>
    [Table("case_person_sentence")]
    public class CasePersonSentence : UserDateWRT , IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("decreed_court_id")]
        [Display(Name = "Постановена от")]
        public int? DecreedCourtId { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Лице")]
        public int CasePersonId { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Акт")]
        public int CaseSessionActId { get; set; }

        [Column("change_case_session_act_id")]
        [Display(Name = "Акт")]
        public int? ChangeCaseSessionActId { get; set; }

        [Column("is_active")]
        [Display(Name = "Активна")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// осъден/оправдан
        /// </summary>
        [Column("sentence_result_type_id")]
        [Display(Name = "Резултат от съдебното производство")]
        public int SentenceResultTypeId { get; set; }

        [Column("description")]
        [Display(Name = "Описание за присъдата")]
        public string Description { get; set; }

        [Column("punishment_activity_id")]
        [Display(Name = "Активност на наказание")]
        public int? PunishmentActivityId { get; set; }

        [Column("punishment_activity_date")]
        [Display(Name = "Дата на активност на наказание")]
        public DateTime? PunishmentActivityDate { get; set; }

        [Display(Name = "Присъда, която е изменена/отменена")]
        [Column("changed_case_person_sentence_id")]
        public int? ChangedCasePersonSentenceId { get; set; }

        //---------------изпълнение на присъда -------------------

        /// <summary>
        /// Дата на влизане в сила на присъдата: след приключване на обжалването
        /// </summary>
        [Display(Name = "Дата на влизане в сила на присъдата")]
        [Column("inforced_date")]
        public DateTime? InforcedDate { get; set; }

        [Display(Name = "Дата на предаване за изпълнение")]
        [Column("for_inforcement_date")]
        public DateTime? ForInforcementDate { get; set; }

        [Display(Name = "Дата изпращане за изпълнение")]
        [Column("sent_date")]
        public DateTime? SentDate { get; set; }

        [Column("sentence_exec_period_id")]
        [Display(Name = "Срок за изпълнение")]
        public int? SentenceExecPeriodId { get; set; }

        /// <summary>
        /// Прокуратури
        /// </summary>
        [Display(Name = "Орган")]
        [Column("inforcer_institution_id")]
        public int? InforcerInstitutionId { get; set; }

        /// <summary>
        /// Описание за изпълнение
        /// </summary>
        [Column("exec_description")]
        [Display(Name = "Текст")]
        public string ExecDescription { get; set; }


        //-------------- Това е после - друга секция
        [Display(Name = "Уведомление за предприети действия по изпълнение")]
        [Column("notification_date")]
        public DateTime? NotificationDate { get; set; }

        [Display(Name = "Дата на привеждане в изпълнение")]
        [Column("exec_date")]
        public DateTime? ExecDate { get; set; }

        [Column("inforcer_document_number")]
        [Display(Name = "Преписка")]
        public string InforcerDocumentNumber { get; set; }

        [Display(Name = "Зачита се от")]
        [Column("effective_date_from")]
        public DateTime? EffectiveDateFrom { get; set; }

        /// <summary>
        /// Затвори
        /// </summary>
        [Display(Name = "Място на изпълнение")]
        [Column("exec_institution_id")]
        public int? ExecInstitutionId { get; set; }

        [Column("amnesty_document_number")]
        [Display(Name = "Номер и дата на указ за помилване")]
        public string AmnestyDocumentNumber { get; set; }

        [Column("exec_remark")]
        [Display(Name = "Забележка")]
        public string ExecRemark { get; set; }

        //---------------изпълнение на присъда -------------------


        [Column("enforce_incoming_document")]
        [Display(Name = "Входящо писмо от орган за приведена присъда")]
        public string EnforceIncomingDocument { get; set; }

        [Column("exec_incoming_document")]
        [Display(Name = "Вх. номер/дата на Писмо за изпълнено наказание")]
        public string ExecIncomingDocument { get; set; }

        [Column("out_document_id")]
        [Display(Name = "Изходящо писмо")]
        public long? OutDocumentId { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }
        
        [ForeignKey(nameof(DecreedCourtId))]
        public virtual Court DecreedCourt { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(ChangeCaseSessionActId))]
        public virtual CaseSessionAct ChangeCaseSessionAct { get; set; }

        [ForeignKey(nameof(SentenceResultTypeId))]
        public virtual SentenceResultType SentenceResultType { get; set; }

        [ForeignKey(nameof(SentenceExecPeriodId))]
        public virtual SentenceExecPeriod SentenceExecPeriod { get; set; }

        [ForeignKey(nameof(InforcerInstitutionId))]
        public virtual Institution InforcerInstitution { get; set; }

        [ForeignKey(nameof(ExecInstitutionId))]
        public virtual Institution ExecInstitution { get; set; }

        [ForeignKey(nameof(PunishmentActivityId))]
        public virtual PunishmentActivity PunishmentActivity { get; set; }

        [ForeignKey(nameof(OutDocumentId))]
        public virtual Document OutDocument { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        public virtual ICollection<CasePersonSentenceLawbase> CasePersonSentenceLawbases { get; set; }
        public virtual ICollection<CasePersonSentencePunishment> CasePersonSentencePunishments { get; set; }
    }
}
