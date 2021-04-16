using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Типове дни 1-работни, 2-Почивни
    /// </summary>
    [Table("nom_day_type")]
    public class DayType : BaseCommonNomenclature
    {
     
    }
}
