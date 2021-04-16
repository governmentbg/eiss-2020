using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Възможнo участие на роля във връзка между страни
    /// </summary>
    [Table("nom_person_role_link_direction")]
    public class PersonRoleLinkDirection
    {
        [Column("person_role_id")]
        public int PersonRoleId { get; set; }

        [Column("link_direction_id")]
        public int LinkDirectionId { get; set; }

        [ForeignKey(nameof(PersonRoleId))]
        public virtual PersonRole PersonRole { get; set; }

        [ForeignKey(nameof(LinkDirectionId))]
        public virtual LinkDirection LinkDirection { get; set; }
    }
}

