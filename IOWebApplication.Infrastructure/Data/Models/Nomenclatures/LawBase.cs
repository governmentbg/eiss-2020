using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Нормативни текстове
    /// </summary>
    [Table("nom_law_base")]
    public class LawBase : BaseCommonNomenclature
    {
        [Column("court_type_id")]
        [Display(Name = "Вид съд")]
        public int? CourtTypeId { get; set; }

        [Column("case_instance_id")]
        [Display(Name = "Инстанция")]
        public int? CaseInstanceId { get; set; }

        [Column("case_group_id")]
        public int? CaseGroupId { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
