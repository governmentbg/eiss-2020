using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на отвод: 1- Уважен,2-Неуважен
    /// </summary>
    [Table("nom_dismissal_state")]
    public class DismissalState : BaseCommonNomenclature
    {

    }
}
