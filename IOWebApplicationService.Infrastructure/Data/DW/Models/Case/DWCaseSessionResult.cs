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
    /// <summary>
    /// Заседания по делото
    /// </summary>
    [Table("dw_case_session_result")]
    public class DWCaseSessionResult : DWUserDateWRT
    {
        [Key]
        [Column("dw_Id")]
        public int dw_Id { get; set; }
    [Column("id")]
    public int Id { get; set; }

   

    [Column("case_id")]
    public int? CaseId { get; set; }

    [Column("case_session_id")]
    public int CaseSessionId { get; set; }

    /// <summary>
    /// Резултат на заседание, отложено, обявен за решаване, спряно ит.н.
    /// </summary>
    [Column("session_result_id")]
    [Display(Name = "Резултат от заседанието")]

    public int SessionResultId { get; set; }

    [Column("session_result_name")]

    public string SessionResultName { get; set; }

    /// <summary>
    ///  Основание за резултат от заседание
    /// </summary>
    [Column("session_result_base_id")]
    [Display(Name = "Основание")]
    public int? SessionResultBaseId { get; set; }

    [Column("session_result_base_name")]
    [Display(Name = "Основание")]
    public string SessionResultBaseName{ get; set; }

    [Column("description")]
    [Display(Name = "Забележка")]
    public string Description { get; set; }

    [Column("is_active")]
    [Display(Name = "Активен резултат")]
    public bool IsActive { get; set; }

    [Column("is_main")]
    [Display(Name = "Основен резултат")]
    public bool IsMain { get; set; }



    [Column("user_expired_id")]
    public string UserExpiredId { get; set; }
    [Column("user_expired_name")]
    public string UserExpiredName{ get; set; }




    [Column("date_expired")]
    [Display(Name = "Дата на анулиране сесия")]
    public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране сесия")]
    public string DateExpiredStr { get; set; }


  }


}
