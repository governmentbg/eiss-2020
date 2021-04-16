using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Съдебен състав по дело - заседатели
    /// </summary>
    [Table("case_lawunit")]
    public class CaseLawUnit : UserDateWRT, ILawUnit
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

        [Column("lawunit_id")]
        [Display(Name = "Служител")]
        public int LawUnitId { get; set; }

        [Column("lawunit_user_id")]
        public string LawUnitUserId { get; set; }

        [Column("judge_role_id")]
        [Display(Name = "Роля")]
        public int JudgeRoleId { get; set; }

        [Column("court_department_id")]
        public int? CourtDepartmentId { get; set; }

        [Column("real_court_department_id")]
        public int? RealCourtDepartmentId { get; set; }

        [Column("court_duty_id")]
        public int? CourtDutyId { get; set; }

        [Column("court_group_id")]
        public int? CourtGroupId { get; set; }

        [Column("judge_department_role_id")]
        public int? JudgeDepartmentRoleId { get; set; }

        [Column("date_from")]
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Column("law_unit_substitution_id")]
        public int? LawUnitSubstitutionId { get; set; }

    [Column("case_selection_protokol_id")]
    public int? CaseSelectionProtokolId { get; set; }

    [ForeignKey(nameof(CaseSelectionProtokolId))]
    public virtual CaseSelectionProtokol CaseSelectionProtokol { get; set; }

    [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(JudgeRoleId))]
        public virtual JudgeRole JudgeRole { get; set; }

        [ForeignKey(nameof(CourtDepartmentId))]
        public virtual CourtDepartment CourtDepartment { get; set; }

        [ForeignKey(nameof(RealCourtDepartmentId))]
        public virtual CourtDepartment RealCourtDepartment { get; set; }

        [ForeignKey(nameof(JudgeDepartmentRoleId))]
        public virtual JudgeDepartmentRole JudgeDepartmentRole { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CourtDutyId))]
        public virtual CourtDuty CourtDuty { get; set; }

        [ForeignKey(nameof(CourtGroupId))]
        public virtual CourtGroup CourtGroup { get; set; }

        public virtual ICollection<CaseLawUnitReplace> LawUnitReplaces { get; set; }

        [ForeignKey(nameof(LawUnitUserId))]
        public virtual ApplicationUser LawUnitUser { get; set; }

        [ForeignKey(nameof(LawUnitSubstitutionId))]
        public virtual CourtLawUnitSubstitution LawUnitSubstitution { get; set; }
    }
}
