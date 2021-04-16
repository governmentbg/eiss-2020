using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Системи за външна интеграция
    /// </summary>
    [Table("nom_integration_type")]
    public class IntegrationType : BaseCommonNomenclature
    {
    }
}
