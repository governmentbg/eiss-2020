using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на резултати от заседание - за справки и на всеки за каквото му трябва
    /// </summary>
    [Table("nom_session_result_base_grouping")]
    public class SessionResultBaseGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("session_result_base_id")]
        public int SessionResultBaseId { get; set; }

        [Column("session_result_base_group")]
        public int SessionResultBaseGroup { get; set; }

        [ForeignKey(nameof(SessionResultBaseId))]
        public virtual SessionResultBase SessionResultBase { get; set; }
    }
}
