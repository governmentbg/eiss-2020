using IOWebApplication.Infrastructure.Data.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Адреси към Лице към електронен документ
    /// </summary>
    [Table("electronic_document_person_address")]
    public class ElectronicDocumentPersonAddress
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("electronic_document_person_id")]
        public long ElectronicDocumentPersonId { get; set; }

        [Column("address_id")]
        public long AddressId { get; set; }

        [ForeignKey(nameof(ElectronicDocumentPersonId))]
        public virtual ElectronicDocumentPerson ElectronicDocumentPerson { get; set; }

        [ForeignKey(nameof(AddressId))]
        public virtual Address Address { get; set; }
    }
}
