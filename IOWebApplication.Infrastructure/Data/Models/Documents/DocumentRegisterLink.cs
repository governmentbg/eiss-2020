using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Връзки на документи към документен регистър
    /// </summary>
    [Table("nom_document_register_links")]
    public class DocumentRegisterLink
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("document_register_id")]
        public int DocumentRegisterId { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("register_no")]
        public int RegisterNo { get; set; }

        [Column("register_date")]
        public DateTime RegisterDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentRegisterId))]
        public virtual DocumentRegister DocumentRegister { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }
    }
}
