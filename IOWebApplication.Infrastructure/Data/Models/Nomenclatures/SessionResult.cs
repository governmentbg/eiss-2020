using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Резултат от заседание
    /// </summary>
    [Table("nom_session_result")]
    public class SessionResult : BaseCommonNomenclature
    {
        [Column("session_result_group_id")]
        public int? SessionResultGroupId { get; set; }

        [ForeignKey(nameof(SessionResultGroupId))]
        public virtual SessionResultGroup SessionResultGroup { get; set; }
    }
}
