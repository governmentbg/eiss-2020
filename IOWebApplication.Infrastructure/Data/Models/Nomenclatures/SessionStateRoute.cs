using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Кой статус след кой следва
    /// </summary>
    [Table("nom_session_state_route")]
    public class SessionStateRoute
    {
        [Column("session_state_from_id")]
        public int SessionStateFromId { get; set; }
        
        [Column("session_state_to_id")]
        public int SessionStateToId { get; set; }

        [ForeignKey(nameof(SessionStateToId))]
        public virtual SessionState SessionStateTo { get; set; }
    }
}
