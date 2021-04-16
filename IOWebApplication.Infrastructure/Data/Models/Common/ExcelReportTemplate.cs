using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
  /// <summary>
  /// Бланки  в Excel формат  за отчети към ВСС
  /// </summary>
  [Table("common_excel_report_template")]
  public class ExcelReportTemplate
  {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("report_type_id")]
    public int ReportTypeId { get; set; }

    [Column("court_type_id")]
    public int CourtTypeId { get; set; }

    [Column("label")]
    [Display(Name = "Наименование")]
    [Required(ErrorMessage = "Въведете {0}.")]
    public string Label { get; set; }

    [Column("description")]
    [Display(Name = "Описание")]
    public string Description { get; set; }

    [Column("date_from")]
    [Display(Name = "Дата от")]
    [Required(ErrorMessage = "Въведете {0}.")]
    public DateTime DateFrom { get; set; }

    [Column("date_to")]
    [Display(Name = "Дата до")]
    public DateTime? DateTo { get; set; }

    [ForeignKey(nameof(CourtTypeId))]
    public virtual CourtType CourtType { get; set; }

   



  }
}
