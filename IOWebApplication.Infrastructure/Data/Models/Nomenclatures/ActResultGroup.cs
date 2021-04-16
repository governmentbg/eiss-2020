using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на Вид на резултат - за справки и на всеки за каквото му трябва. Вече има Grouping затова е Group
    /// </summary>
    [Table("nom_act_result_group")]
    public class ActResultGroup
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("act_result_id")]
        public int ActResultId { get; set; }

        [Column("act_result_grouping")]
        public int ActResultGrouping { get; set; }

        [ForeignKey(nameof(ActResultId))]
        public virtual ActResult ActResult { get; set; }
    }
}
