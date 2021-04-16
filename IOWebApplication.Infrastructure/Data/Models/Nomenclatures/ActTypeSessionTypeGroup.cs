using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    // Таблица за филтриране на тип акт по група на заседание
    [Table("nom_act_type_session_type_group")]
    public class ActTypeSessionTypeGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("act_type_id")]
        public int ActTypeId { get; set; }

        [Column("session_type_group")]
        public int SessionTypeGroup { get; set; }

        [ForeignKey(nameof(ActTypeId))]
        public virtual ActType ActType { get; set; }
    }
}
