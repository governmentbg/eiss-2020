using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Индекси за статистиката по тип съд, тип на акт и вид дело
    /// </summary>
    [Table("nom_excel_report_index")]
    public class ExcelReportIndex
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [Column("case_type_id")]
        public string CaseTypeId { get; set; }

        [Column("act_type_id")]
        public string ActTypeId { get; set; }

        [Column("col_index")]
        public int ColIndex { get; set; }

        [Column("act_complain_index")]
        public string ActComplainIndex { get; set; }

        [Column("case_group_id")]
        public int? CaseGroupId { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
