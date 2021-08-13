using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.VKS
{
  /// <summary>
  /// Участници към Полугодишен избор на съдии за ВКС
  /// </summary>
  [Table("vks_selection_lawunit")]
  public class VksSelectionLawunit : UserDateWRT, IExpiredInfo
  {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("replaced_lawunit_id")]
    public int? ReplacedLawunitId { get; set; }

    [Column("vks_selection_id")]
    public int VksSelectionId { get; set; }

    [Column("lawunit_id")]
    public int? LawunitId { get; set; }

    /// <summary>
    /// Отделение, колегия на съдията
    /// </summary>
    [Column("court_department_type_id")]
    [Display(Name = "Структура")]
    public int? CourtDepartmentTypeId { get; set; }

    /// <summary>
    /// Уникален GUID за всеки нов оригинален съдия или ключа на заместения съдия
    /// </summary>
    [Column("lawunit_key")]
    public string LawunitKey { get; set; }

    [Column("lawunit_name")]
    public string LawunitName { get; set; }

    [Column("is_unknown_judge")]
    public bool IsUnknownJudge { get; set; }

    [Column("judge_department_role_id")]
    [Display(Name = "Роля")]
    public int? JudgeDepartmentRoleId { get; set; }

    [Display(Name = "Начална дата")]
    [Column("date_start")]
    public DateTime DateStart { get; set; }

    [Display(Name = "Крайна дата")]
    [Column("date_end")]
    public DateTime? DateEnd { get; set; }

    [Column("date_expired")]
    [Display(Name = "Дата на анулиране")]
    public DateTime? DateExpired { get; set; }

    [Column("user_expired_id")]
    public string UserExpiredId { get; set; }

    [Column("description_expired")]
    [Display(Name = "Причина за анулиране")]
    public string DescriptionExpired { get; set; }
    [NotMapped]
    public int OrderNumber { get; set; }

    [ForeignKey(nameof(UserExpiredId))]
    public virtual ApplicationUser UserExpired { get; set; }

    [ForeignKey(nameof(ReplacedLawunitId))]
    public virtual VksSelectionLawunit ReplacedLawunit { get; set; }

    [ForeignKey(nameof(VksSelectionId))]
    public virtual VksSelection VksSelection { get; set; }

    [ForeignKey(nameof(LawunitId))]
    public virtual LawUnit LawUnit { get; set; }

    [ForeignKey(nameof(JudgeDepartmentRoleId))]
    public virtual JudgeDepartmentRole JudgeDepartmentRole { get; set; }

    [ForeignKey(nameof(CourtDepartmentTypeId))]
    public virtual DepartmentType DepartmentType { get; set; }
  }
}
