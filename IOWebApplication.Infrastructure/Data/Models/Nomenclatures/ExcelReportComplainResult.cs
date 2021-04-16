using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// ActComplainResult за статистиката по тип съд, sheetindex
    /// </summary>
    [Table("nom_excel_report_complain_result")]
    public class ExcelReportComplainResult
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [Column("sheet_index")]
        public int SheetIndex { get; set; }

        [Column("act_complain_result")]
        public string ActComplainResult { get; set; }

        [Column("col_index")]
        public int ColIndex { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

    }
}
