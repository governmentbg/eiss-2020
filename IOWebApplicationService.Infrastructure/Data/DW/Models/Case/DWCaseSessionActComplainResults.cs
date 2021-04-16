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
  [Table("dw_case_session_act_complain_result")]
  public class DWCaseSessionActComplainResult : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
 
    public int Id { get; set; }

    [Column("case_session_act_complain_id")]
    public int CaseSessionActComplainId { get; set; }

    /// <summary>
    /// Обжалва се пред, съд
    /// </summary>
    

    [Column("case_id")]

    [Display(Name = "Дело")]

    public int CaseId { get; set; }

    [Column("case_session_id")]
   
    public int? CaseSessionId { get; set; }
    [Column("case_short_number_value")]
    public int? CaseShortNumberValue { get; set; }

    /// <summary>
    ///Пълен 14 цифрен номер на делото
    /// </summary>
    [Column("case_reg_number")]
    public string CaseRegNumber { get; set; }

    [Column("case_reg_date")]
    public DateTime CaseRegDate { get; set; }

    [Column("case_session_act_id")]

    [Display(Name = "Акт")]
    public int CaseSessionActId { get; set; }

    [Column("act_result_id")]
    [Display(Name = "Резултат от обжалване")]

    public int? ActResultId { get; set; }

    [Column("act_result_name")]
    [Display(Name = "Резултат от обжалване")]

    public string ActResultName { get; set; }

    [Column("description")]
    [Display(Name = "Описание")]
    public string Description { get; set; }

    [Column("date_result")]
    [Display(Name = "Дата на отразяване на резултат")]
    public DateTime? DateResult { get; set; }


    [Column("date_expired")]
    [Display(Name = "Дата на анулиране сесия")]
    public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране сесия")]
    public string DateExpiredStr { get; set; }

  }
}
