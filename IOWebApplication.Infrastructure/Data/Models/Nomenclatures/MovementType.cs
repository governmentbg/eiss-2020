using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид местоположение на дело: към лице,към структура от организацията или към външна организация
    /// </summary>
    [Table("nom_movement_type")]
    public class MovementType : BaseCommonNomenclature
    {
    }
}
