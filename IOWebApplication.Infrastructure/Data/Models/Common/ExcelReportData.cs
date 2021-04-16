using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// данни за Бланки  в Excel формат  за отчети към ВСС
    /// </summary>
    [Table("common_excel_report_data")]
    public class ExcelReportData : UserDateWRT
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("excel_report_template_id")]
        public int ExcelReportTemplateId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("report_month")]
        public int ReportMonth { get; set; }

        [Column("report_year")]
        public int ReportYear { get; set; }

        [Column("sheet_index")]
        public int SheetIndex { get; set; }

        [Column("row_index")]
        public int RowIndex { get; set; }

        [Column("row_data_col_index")]
        public int RowDataColIndex { get; set; }

        [Column("row_data")]
        public string RowData { get; set; }

        [Column("col_index")]
        public int ColIndex { get; set; }

        [Column("cell_value")]
        public string CellValue { get; set; }

        [Column("cell_value_int")]
        public int? CellValueInt { get; set; }

        //1 - String, 2 - Int
        [Column("cell_value_type")]
        public int? CellValueType { get; set; }

        [ForeignKey(nameof(ExcelReportTemplateId))]
        public virtual ExcelReportTemplate ExcelReportTemplate { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

    }
}
