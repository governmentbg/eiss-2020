using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Филтри по бланки в Excel формат за отчети към ВСС
    /// </summary>
    [Table("common_excel_report_case_filter")]
    public class ExcelReportCaseFilter
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("excel_report_template_id")]
        public int ExcelReportTemplateId { get; set; }
      
        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [Column("case_code_id")]
        public int? CaseCodeId { get; set; }

        [Column("sheet_index")]
        public int SheetIndex { get; set; }

        [Column("row_index")]
        public int RowIndex { get; set; }

        [ForeignKey(nameof(ExcelReportTemplateId))]
        public virtual ExcelReportTemplate ExcelReportTemplate { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }
    }
}
