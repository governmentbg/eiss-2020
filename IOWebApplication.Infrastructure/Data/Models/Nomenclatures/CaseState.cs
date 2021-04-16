using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на дело
    /// </summary>
    [Table("nom_case_state")]
    public class CaseState : BaseCommonNomenclature
    {
        [Column("is_initial_state")]
        public bool? IsInitialState { get; set; }
    }
}
