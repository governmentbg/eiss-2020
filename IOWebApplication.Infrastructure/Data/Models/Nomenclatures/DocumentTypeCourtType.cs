using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид съд по иницииращи точни видове документи
    /// </summary>
    [Table("nom_document_type_court_type")]
    public class DocumentTypeCourtType
    {
        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }
    }
}
