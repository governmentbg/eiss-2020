using IOWebApplication.Infrastructure.Data.Models.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групи съдебни актове - за номериране
    /// </summary>
    [Table("nom_session_act_group")]
    public class SessionActGroup : BaseCommonNomenclature
    {
        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        //[InverseProperty(nameof(CounterSessionAct.SessionActGroupId))]
        public virtual ICollection<CounterSessionAct> CounterSessionActs { get; set; }
    }
}
