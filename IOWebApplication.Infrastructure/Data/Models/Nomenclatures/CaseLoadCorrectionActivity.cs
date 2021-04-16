using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Коригиращи индекси за трудност на дело - дейности
    /// </summary>
    [Table("nom_case_load_correction_activity")]
    public class CaseLoadCorrectionActivity : BaseCommonNomenclature
    {
        [Column("case_group_id")]
        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        /// <summary>
        /// Елементи с еднакви групи не могат да се дублират
        /// </summary>
        [Column("correction_group")]
        [Display(Name = "Група")]
        public int? CorrectionGroup { get; set; }

        [Column("court_type_id")]
        [Display(Name = "Инстанция")]
        public int CaseInstanceId { get; set; }

        [Column("load_index")]
        [Display(Name = "Индекс")]
        public decimal LoadIndex { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }
    }
}
