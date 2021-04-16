using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основен вид документ
    /// </summary>
    [Table("nom_document_group")]
    public class DocumentGroup : BaseCommonNomenclature
    {
        [Column("document_kind_id")]
        [Display(Name = "Вид документ")]

        public int DocumentKindId { get; set; }

        [ForeignKey(nameof(DocumentKindId))]
        public virtual DocumentKind DocumentKind { get; set; }
    }
}
