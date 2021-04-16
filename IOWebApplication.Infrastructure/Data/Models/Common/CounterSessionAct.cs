using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Връзки регистър съдебни актове към брояч
    /// </summary>
    [Table("common_counter_session_act")]
    public class CounterSessionAct
    {
       
        [Column("counter_id")]
        public int CounterId { get; set; }

        [Column("session_act_group_id")]
        public int SessionActGroupId { get; set; }

        [ForeignKey(nameof(CounterId))]
        public virtual Counter Counter { get; set; }

        [ForeignKey(nameof(SessionActGroupId))]
        public virtual SessionActGroup SessionActGroup { get; set; }

    }
}
