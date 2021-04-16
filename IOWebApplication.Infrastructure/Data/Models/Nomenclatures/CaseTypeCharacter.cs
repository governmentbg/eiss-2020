using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Връзки точен вид към характер
    /// </summary>
    [Table("nom_case_type_character")]
    public class CaseTypeCharacter
    {
        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [Column("case_character_id")]
        public int CaseCharacterId { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        [Display(Name = "Точен вид дело")]
        public virtual CaseType CaseType { get; set; }

        [ForeignKey(nameof(CaseCharacterId))]
        public virtual CaseCharacter CaseCharacter { get; set; }
    }
}
