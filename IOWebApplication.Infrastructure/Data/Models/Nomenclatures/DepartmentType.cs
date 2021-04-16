using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид ниво: колегия, отделение, съд (за малките)
    /// </summary>
    [Table("nom_department_type")]
    public class DepartmentType : BaseCommonNomenclature
    {
    }
}
