using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Натовареност по дела - допълнителни дейности
    /// </summary>
    [Table("nom_case_load_add_activity")]
    public class CaseLoadAddActivity : BaseCommonNomenclature
    {
        [Column("is_ND")]
        [Display(Name = "Наказателно дело")]
        public bool IsND { get; set; }
    }
}
