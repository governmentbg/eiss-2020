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

        /// <summary>
        /// Пълен член
        /// </summary>
        [Column("big_forum")]
        public string BigForum { get; set; }

        /// <summary>
        /// Кратък член
        /// </summary>
        [Column("short_forum")]
        public string ShortForum { get; set; }

        /// <summary>
        /// Подлежащи на правна помощ
        /// </summary>
        [Column("for_lawyer_help")]
        public bool ForLawyerHelp { get; set; }

        /// <summary>
        /// Са се добави в призоваване с държавен вестник като лява страна
        /// </summary>
        [Column("for_vks_as_left_side")]
        public bool ForVksAsLeftSide { get; set; }

        /// <summary>
        /// Са се добави в призоваване с държавен вестник като дясна страна
        /// </summary>
        [Column("for_vks_as_right_side")]
        public bool ForVksAsRightSide { get; set; }

        [ForeignKey(nameof(RoleKindId))]
        public virtual RoleKind RoleKind { get; set; }
    }
}
