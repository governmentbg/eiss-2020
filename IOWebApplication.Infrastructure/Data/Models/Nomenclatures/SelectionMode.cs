using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Случайно разпределяне: Начин на разпределение:1-автоматично,2-ръчно,3-по дежурство
    /// </summary>
    [Table("nom_selection_mode")]
    public class SelectionMode : BaseCommonNomenclature
    {
        
    }
}
