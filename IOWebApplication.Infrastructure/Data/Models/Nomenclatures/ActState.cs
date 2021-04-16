using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на съдебен акт
    /// </summary>
    [Table("nom_act_state")]
    public class ActState : BaseCommonNomenclature
    {
        [Column("is_initial_state")]
        public bool? IsInitialState { get; set; }
    }
}
