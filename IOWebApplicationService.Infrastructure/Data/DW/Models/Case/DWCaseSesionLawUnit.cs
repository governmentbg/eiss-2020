using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.DW.Models;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
  /// <summary>
  /// Съдебен състав по дело - заседатели
  /// </summary>
  [Table("dw_case_session_lawunit")]
  public class DWCaseSessionLawUnit: DWUserDateWRT
  {
    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }
    [Column("id")]
    public int Id { get; set; }

    [Column("case_id")]
    public int CaseId { get; set; }

    [Column("case_session_id")]
    public int? CaseSessionId { get; set; }

    [Column("lawunit_id")]
    public int LawUnitId { get; set; }

    [Column("lawunit_full_name")]
    public string LawUnitFullName { get; set; }


    [Column("judge_role_id")]
    public int JudgeRoleId { get; set; }
    [Column("judge_role_name")]
    public string JudgeRoleName { get; set; }

    [Column("court_department_id")]
    public int? CourtDepartmentId { get; set; }

    [Column("court_department_name")]
    public string CourtDepartmentName { get; set; }

    [Column("court_duty_id")]
    public int? CourtDutyId { get; set; }
    [Column("court_duty_name")]
    public string CourtDutyName { get; set; }

    [Column("court_group_id")]
    public int? CourtGroupId { get; set; }
    [Column("court_group_name")]
    public string CourtGroupName { get; set; }

    [Column("judge_department_role_id")]
    public int? JudgeDepartmentRoleId { get; set; }
    [Column("judge_department_role_name")]
    public string JudgeDepartmentRoleName { get; set; }

    [Column("date_from")]
    public DateTime DateFrom { get; set; }

    [Column("date_to")]
    public DateTime? DateTo { get; set; }
    [Column("date_from_str")]
    public String DateFromStr { get; set; }

    [Column("date_to_str")]

    public String DateToStr { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("date_expired")]
    [Display(Name = "Дата на анулиране сесия")]
    public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране сесия")]
    public string DateExpiredStr { get; set; }





  }
}
