using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид институция: 1-съд,2-прокуратура,3-од мвр,чси,нотариуси и други
    /// </summary>
    [Table("nom_institution_type")]
    public class InstitutionType : BaseCommonNomenclature
    {
        [Column("eispp_code_label")]
        public string EISPPcodeLabel { get; set; }
    }
}
