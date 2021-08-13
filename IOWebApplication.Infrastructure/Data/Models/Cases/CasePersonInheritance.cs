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
    /// Наследство по лице в дело
    /// </summary>
    [Table("case_person_inheritance")]
    public class CasePersonInheritance : UserDateWRT, IExpiredInfo
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

        [Column("case_person_inheritance_result_id")]
        [Display(Name = "Резултат")]
        public int CasePersonInheritanceResultId { get; set; }

        [Column("is_active")]
        [Display(Name = "Активна")]
        public bool? IsActive { get; set; }

        [Column("description")]
        [Display(Name = "Описание за присъдата")]
        public string Description { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

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

        [ForeignKey(nameof(CasePersonInheritanceResultId))]
        public virtual CasePersonInheritanceResult CasePersonInheritanceResult { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
    }
}
