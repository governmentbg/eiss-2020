using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на Резултат / Степен на уважаване на иска - за справки и на всеки за каквото му трябва
    /// </summary>
    [Table("nom_act_complain_result_grouping")]
    public class ActComplainResultGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("act_complain_result_id")]
        public int ActComplainResultId { get; set; }

        [Column("act_complain_result_group")]
        public int ActComplainResultGroup { get; set; }

        [ForeignKey(nameof(ActComplainResultId))]
        public virtual ActComplainResult ActComplainResult { get; set; }
    }
}
