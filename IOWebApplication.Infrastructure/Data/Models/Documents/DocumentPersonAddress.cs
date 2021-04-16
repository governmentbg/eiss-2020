using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Адреси към Лице към деловоден документ
    /// </summary>
    [Table("document_person_address")]
    public class DocumentPersonAddress
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("document_person_id")]
        public long DocumentPersonId { get; set; }

        [Column("address_id")]
        public long AddressId { get; set; }

        [ForeignKey(nameof(DocumentPersonId))]
        public virtual DocumentPerson DocumentPerson { get; set; }

        [ForeignKey(nameof(AddressId))]
        public virtual Address Address { get; set; }
    }
}
