using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид документ: Иницииращ документ, Съпровождащ, Обща администрация
    /// </summary>
    [Table("nom_document_kind")]
    public class DocumentKind : BaseCommonNomenclature
    {
        /// <summary>
        /// Посока на движение на документ: Входящи, Изходящи, вътрешен
        /// </summary>
        [Column("document_direction_id")]
        [Display(Name = "Направление")]
        public int DocumentDirectionId { get; set; }

        [ForeignKey(nameof(DocumentDirectionId))]
        public virtual DocumentDirection DocumentDirection { get; set; }
    }
}
