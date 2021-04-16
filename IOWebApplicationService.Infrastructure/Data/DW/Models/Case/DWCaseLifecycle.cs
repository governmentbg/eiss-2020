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
    [Table("dw_case_lifecycle")]
    public class DWCaseLifecycle : DWUserDateWRT
    {
        [Key]
        [Column("dw_Id")]
        public int dw_Id { get; set; }

        [Column("id")]
        public int Id { get; set; }


    [Column("case_id")]
    public int CaseId { get; set; }

    /// <summary>
    /// 1-разглежда се,2-спряно
    /// </summary>
    [Column("lifecycle_type_id")]
    [Display(Name = "Вид интервал")]
    public int LifecycleTypeId { get; set; }
    [Column("lifecycle_type_id_name")]
    [Display(Name = "Вид интервал")]
    public string LifecycleTypeIdName { get; set; }

    [Column("iteration")]
    [Display(Name = "Повторение")]
    public int Iteration { get; set; }

    [Column("date_from")]
    [Display(Name = "От дата")]
    public DateTime DateFrom { get; set; }
    [Column("date_from_str")]
    [Display(Name = "От дата")]
    public string DateFromStr { get; set; }

    [Column("date_to")]
    [Display(Name = "До дата")]
    public DateTime? DateTo { get; set; }
    [Column("date_to_str")]
    [Display(Name = "До дата")]
    public string DateToStr { get; set; }

    [Column("duration_months")]
    [Display(Name = "Продължителност в месеци")]
    public int DurationMonths { get; set; }

    [Column("description")]
    [Display(Name = "Забележка")]
    public string Description { get; set; }

  }


}
