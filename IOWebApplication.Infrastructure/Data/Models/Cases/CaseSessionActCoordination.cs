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
    /// Съгласуване на актове
    /// </summary>
    [Table("case_session_act_coordination")]
    public class CaseSessionActCoordination : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_session_act_id")]
        public int CaseSessionActId { get; set; }

        [Column("case_lawunit_id")]
        [Display(Name = "Съдия")]
        public int CaseLawUnitId { get; set; }

        [Column("act_coordination_type_id")]
        [Display(Name = "Статус")]
        public int ActCoordinationTypeId { get; set; }

        /// <summary>
        /// Вид съгласуване: 1 - на акт,2 - на мотиви
        /// </summary>
        [Column("coordination_type")]
        public int CoordinationType { get; set; }

        /// <summary>
        /// Дата на подписване на особеното мнение
        /// </summary>
        [Column("coordination_declared_date")]
        public DateTime? CoordinationDeclaredDate { get; set; }

        /// <summary>
        /// Потребител обезличил акта
        /// </summary>
        [Column("depersonalize_user_id")]
        public string DepersonalizeUserId { get; set; }

        /// <summary>
        /// Дата на финализиране на обезличаването
        /// </summary>
        [Column("depersonalize_end_date")]
        public DateTime? DepersonalizeEndDate { get; set; }

        [Column("content")]
        [Display(Name = "Особено мнение")]
        [AllowHtml]
        public string Content { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(CaseLawUnitId))]
        public virtual CaseLawUnit CaseLawUnit { get; set; }

        [ForeignKey(nameof(ActCoordinationTypeId))]
        public virtual ActCoordinationType ActCoordinationType { get; set; }

        [ForeignKey(nameof(DepersonalizeUserId))]
        public virtual ApplicationUser DepersonalizeUser { get; set; }
    }
}
