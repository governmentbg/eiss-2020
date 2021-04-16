using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на заявка за трансфер
    /// </summary>
    [Table("nom_integration_state")]
    public class IntegrationState : BaseCommonNomenclature
    {
    }
}
