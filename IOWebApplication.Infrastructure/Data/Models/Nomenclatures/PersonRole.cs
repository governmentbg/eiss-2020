using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Роля на лицето, ищец,ответник,свидетел,експерт
    /// </summary>
    [Table("nom_person_role")]
    public class PersonRole : BaseCommonNomenclature
    {
        /// <summary>
        /// Представлявана страна/Преставител
        /// </summary>
        [Column("role_kind_id")]
        public int RoleKindId { get; set; }

        [ForeignKey(nameof(RoleKindId))]
        public virtual RoleKind RoleKind { get; set; }
    }
}
