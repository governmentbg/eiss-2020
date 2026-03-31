using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// мапинг между статиската и сисма
    /// </summary>
    [Table("nom_excel_sisma_mapping")]
    public class ExcelSismaMapping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [Column("sheet_index")]
        public int SheetIndex { get; set; }

        [Column("row_index")]
        public int RowIndex { get; set; }

        [Column("col_index")]
        public int ColIndex { get; set; }

        [Column("sisma_index")]
        public string SismaIndex { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }
    }
}
