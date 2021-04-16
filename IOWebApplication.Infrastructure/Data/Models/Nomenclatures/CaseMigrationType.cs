using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид движение на дело между инстанции: По подсъдност, за обжалване,връщане, по компетентност
    /// </summary>
    [Table("nom_case_migration_type")]
    public class CaseMigrationType : BaseCommonNomenclature
    {
        /// <summary>
        /// 1-изходящи движения;2-входящи движения - константи
        /// </summary>
        [Column("migration_direction")]
        public int MigrationDirection { get; set; }

        /// <summary>
        /// id на предходен вид движение - свързване на изпращане и приемане 
        /// </summary>
        [Column("prior_migration_type_id")]
        public int? PriorMigrationTypeId { get; set; }
    }
}
