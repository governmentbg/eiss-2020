using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статистика Данни по CaseType
    /// </summary>
    [Table("nom_excel_report_case_type_row")]
    public class ExcelReportCaseTypeRow
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        //CaseType отделени със запетая
        [Column("case_type_id")]
        public string CaseTypeId { get; set; }

        //Видове документи отделени със запетая
        [Column("document_type_id")]
        public string DocumentTypeId { get; set; }


        //CaseCode отделени със запетая
        [Column("case_code_id")]
        public string CaseCodeId { get; set; }

        [Column("row_index")]
        public int RowIndex { get; set; }

        //За кои колони важи - има колони които се взимат от другите sheets и не трябва да се попълват
        [Column("for_columns")]
        public string ForColumns { get; set; }

        [Column("is_true")]
        public bool IsTrue { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
