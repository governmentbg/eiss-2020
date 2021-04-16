using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
  [Table("dw_case_session_act_coordination")]
  public class DWCaseSessionActCoordination : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

  
    [Column("id")]
    public int Id { get; set; }

    [Column("case_session_act_id")]
    public int CaseSessionActId { get; set; }

    [Column("case_lawunit_id")]
    [Display(Name = "Съдия")]
    public int CaseLawUnitId { get; set; }

    [Column("case_lawunit_name")]
    [Display(Name = "Съдия")]
    public string CaseLawUnitName { get; set; }

    [Column("act_coordination_type_id")]
    [Display(Name = "Статус")]
    public int ActCoordinationTypeId { get; set; }
    [Column("act_coordination_type_name")]
    [Display(Name = "Статус")]
    public string ActCoordinationTypeName { get; set; }
    [Column("content")]
    [Display(Name = "Особено мнение")]
    public string Content { get; set; }


    [Column("date_expired")]
    [Display(Name = "Дата на анулиране сесия")]
    public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране сесия")]
    public string DateExpiredStr { get; set; }

  }
}
