using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Елементи към група за натовареност по дела - основни дейности
    /// </summary>
    [Table("nom_case_load_element_type")]
    public class CaseLoadElementType : BaseCommonNomenclature
    {
        [Column("case_load_element_group_id")]
        public int CaseLoadElementGroupId { get; set; }

        [Column("load_procent")]
        [Display(Name = "Процент")]
        public decimal LoadProcent { get; set; }

        [Column("replace_case_load_element_type_id")]
        [Display(Name = "Елемент за заместване")]
        public int? ReplaceCaseLoadElementTypeId { get; set; }

        [ForeignKey(nameof(CaseLoadElementGroupId))]
        public virtual CaseLoadElementGroup CaseLoadElementGroup { get; set; }

        [ForeignKey(nameof(ReplaceCaseLoadElementTypeId))]
        public virtual CaseLoadElementType ReplaceCaseLoadElementType { get; set; }

        public virtual ICollection<CaseLoadElementTypeRule> CaseLoadElementTypeRules { get; set; }
    }
}
