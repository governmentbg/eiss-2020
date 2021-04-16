using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на Роли - за справки и функционалности
    /// </summary>
    [Table("nom_person_role_grouping")]
    public class PersonRoleGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("person_role_id")]
        public int PersonRoleId { get; set; }

        [Column("person_role_group")]
        public int PersonRoleGroup { get; set; }

        [ForeignKey(nameof(PersonRoleId))]
        public virtual PersonRole PersonRole { get; set; }
    }
}
