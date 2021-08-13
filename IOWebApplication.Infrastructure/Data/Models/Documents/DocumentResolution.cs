using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Разпореждания по документ
    /// </summary>
    [Table("document_resolution")]
    public class DocumentResolution : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("decision_type_id")]
        [Display(Name = "Тип")]
        public int ResolutionTypeId { get; set; }

        [Column("reg_number")]
        [Display(Name = "Номер")]
        public string RegNumber { get; set; }

        [Column("reg_date")]
        [Display(Name = "Дата")]
        public DateTime? RegDate { get; set; }

        /// <summary>
        /// Дата на постановяване: след подписване
        /// </summary>
        [Display(Name = "Дата на постановяване")]
        [Column("declared_date")]
        public DateTime? DeclaredDate { get; set; }

        [Column("judge_decision_count")]
        public int? JudgeDecisionCount { get; set; }

        [Column("judge_decision_lawunit_id")]
        [Display(Name = "Съдия")]
        public int JudgeDecisionLawunitId { get; set; }

        [Column("judge_decision_user_id")]
        public string JudgeDecisionUserId { get; set; }

        [Column("judge_decision_lawunit2_id")]
        [Display(Name = "Съдия")]
        public int? JudgeDecisionLawunit2Id { get; set; }

        [Column("judge_decision_user2_id")]
        public string JudgeDecisionUser2Id { get; set; }

        [Column("user_decision_id")]
        [Display(Name = "Изготвил")]
        public string UserDecisionId { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("document_decision_state_id")]
        [Display(Name = "Статус")]
        public int ResolutionStateId { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [Column("task_user_id")]
        [Display(Name = "Изпълнител")]
        public string TaskUserId { get; set; }


        /// <summary>
        /// Потребител, създал бланката на акта
        /// </summary>
        [Column("act_creator_user_id")]
        public string ActCreatorUserId { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(TaskUserId))]
        public virtual ApplicationUser TaskUser { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(ResolutionTypeId))]
        public virtual ResolutionType ResolutionType { get; set; }


        [ForeignKey(nameof(JudgeDecisionLawunitId))]
        public virtual LawUnit JudgeDecisionLawunit { get; set; }

        [ForeignKey(nameof(JudgeDecisionUserId))]
        public virtual ApplicationUser JudgeDecisionUser { get; set; }

        [ForeignKey(nameof(JudgeDecisionLawunit2Id))]
        public virtual LawUnit JudgeDecisionLawunit2 { get; set; }

        [ForeignKey(nameof(JudgeDecisionUser2Id))]
        public virtual ApplicationUser JudgeDecisionUser2 { get; set; }

        [ForeignKey(nameof(UserDecisionId))]
        public virtual ApplicationUser UserDecision { get; set; }

        [ForeignKey(nameof(ActCreatorUserId))]
        public virtual ApplicationUser ActCreatorUser { get; set; }

        [ForeignKey(nameof(ResolutionStateId))]
        public virtual ResolutionState ResolutionState { get; set; }

        public virtual ICollection<DocumentResolutionCase> Cases { get; set; }
    }
}
