using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Данни за съществуващи документи, свързани към деловоден документ
    /// Изходящи към входящи
    /// </summary>
    [Table("document_link")]
    public class DocumentLink
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        /// <summary>
        /// Посока на движение на документ: Входящи, Изходящи, вътрешен
        /// </summary>
        [Column("document_direction_id")]
        public int? DocumentDirectionId { get; set; }

        /// <summary>
        /// Предходен документ ID
        /// </summary>
        [Column("prev_document_id")]
        public long? PrevDocumentId { get; set; }

        /// <summary>
        /// Предходен документ номер
        /// </summary>
        [Column("prev_document_number")]
        public string PrevDocumentNumber { get; set; }

        /// <summary>
        /// Предходен документ дата
        /// </summary>
        [Column("prev_document_date")]
        public DateTime? PrevDocumentDate { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentDirectionId))]
        public virtual DocumentDirection DocumentDirection { get; set; }

        [ForeignKey(nameof(PrevDocumentId))]
        public virtual Document PrevDocument { get; set; }
    }
}
