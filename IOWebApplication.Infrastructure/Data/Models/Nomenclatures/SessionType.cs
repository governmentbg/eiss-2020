using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид заседание
    /// </summary>
    [Table("nom_session_type")]
    public class SessionType : BaseCommonNomenclature
    {
        [Column("session_type_group")]
        public int? SessionTypeGroup { get; set; }

        /// <summary>
        /// Вид заседание за бланката на съдебните актове
        /// 10.05.2020
        /// </summary>
        [Column("session_act_label")]
        public string SessionActLabel { get; set; }
    }
}
