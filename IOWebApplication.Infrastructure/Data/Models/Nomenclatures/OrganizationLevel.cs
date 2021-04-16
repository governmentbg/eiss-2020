using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид организационно ниво: отдел, дирекция, сектор и т.н.
    /// </summary>
    [Table("nom_organization_level")]
    public class OrganizationLevel : BaseCommonNomenclature
    {
    }
}
