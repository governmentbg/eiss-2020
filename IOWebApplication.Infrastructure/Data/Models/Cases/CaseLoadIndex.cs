using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Натовареност по дела: основни и допълнителни дейности
    /// </summary>
    [Table("case_load_index")]
    public class CaseLoadIndex : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int? CaseSessionId { get; set; }

        [Column("session_type_id")]
        public int? SessionTypeId { get; set; }

        [Display(Name = "Акт")]
        [Column("case_session_act_id")]
        public int? CaseSessionActId { get; set; }

        [Column("act_type_id")]
        public int? ActTypeId { get; set; }

        [Column("case_session_result_id")]
        public int? CaseSessionResultId { get; set; }

        [Column("session_result_id")]
        public int? SessionResultId { get; set; }

        [Column("lawunit_id")]
        [Display(Name = "Съдия")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int LawUnitId { get; set; }
        
        [Column("date_activity")]
        [Required(ErrorMessage = "Въведете {0}.")]
        [Display(Name = "Дата")]
        public DateTime DateActivity { get; set; }

        /// <summary>
        /// true- основни дейности,false - допълнителни
        /// </summary>
        [Column("is_main_activity")]
        public bool IsMainActivity { get; set; }

        //-----------Основни дейности----------------
        [Display(Name = "Група")]
        [Column("case_load_element_group_id")]
        public int? CaseLoadElementGroupId { get; set; }

        [Column("case_load_element_type_id")]
        [Display(Name = "Елемент")]
        public int? CaseLoadElementTypeId { get; set; }
        //-----------Основни дейности----------------

        //-----------Допълнителни дейности----------------
        [Column("case_load_add_activity_id")]
        [Display(Name = "Група")]
        public int? CaseLoadAddActivityId { get; set; }
        //-----------Допълнителни дейности----------------

        [Column("base_index")]
        public decimal BaseIndex { get; set; }

        [Column("load_procent")]
        public decimal LoadProcent { get; set; }

        [Column("load_index")]
        public decimal LoadIndex { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(CaseLoadElementGroupId))]
        public virtual CaseLoadElementGroup CaseLoadElementGroup { get; set; }

        [ForeignKey(nameof(CaseLoadElementTypeId))]
        public virtual CaseLoadElementType CaseLoadElementType { get; set; }

        [ForeignKey(nameof(CaseLoadAddActivityId))]
        public virtual CaseLoadAddActivity CaseLoadAddActivity { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CaseSessionResultId))]
        public virtual CaseSessionResult CaseSessionResult { get; set; }

        [ForeignKey(nameof(SessionResultId))]
        public virtual SessionResult SessionResult { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(SessionTypeId))]
        public virtual SessionType SessionType { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(ActTypeId))]
        public virtual ActType ActType { get; set; }
    }
}
