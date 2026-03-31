using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове шаблони за бланки
    /// </summary>
    [Table("nom_blank_template")]
    public class BlankTemplate : BaseCommonNomenclature
    {
        /// <summary>
        /// Основен тип обект: 7-акт,104-Разпореждане
        /// </summary>
        [Column("source_type")]
        [Display(Name = "Основен вид")]
        public int SourceType { get; set; }

        /// <summary>
        /// Вид акт: ActTypeId или DocumentResolutionTypeId
        /// </summary>
        [Display(Name = "Тип")]
        [Column("source_id")]
        public int SourceId { get; set; }

        [AllowHtml]
        [Column("main_text")]
        public string MainText { get; set; }

        [AllowHtml]
        [Column("add_text")]
        public string AddText { get; set; }
    }
}
