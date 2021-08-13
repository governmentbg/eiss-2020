using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статистика Основание по ТЗ
    /// </summary>
    [Table("nom_excel_report_act_ispn_reason")]
    public class ExcelReportActIspnReason
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [Column("sheet_index")]
        public int SheetIndex { get; set; }

        [Column("act_ispn_reason")]
        public string ActIspnReason { get; set; }

        [Column("col_index")]
        public int ColIndex { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
