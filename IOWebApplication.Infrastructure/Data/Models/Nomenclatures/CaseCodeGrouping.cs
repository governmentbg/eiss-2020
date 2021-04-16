using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на шифри - за справки и на всеки за каквото му трябва
    /// </summary>
    [Table("nom_case_code_grouping")]
    public class CaseCodeGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("case_code_id")]
        public int CaseCodeId { get; set; }

        [Column("case_code_group")]
        public int CaseCodeGroup { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }
    }
}
