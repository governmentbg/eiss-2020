using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Лица към деловоден документ (Кореспондент)
    /// </summary>
    [Table("document_person")]
    public class DocumentPerson : PersonNamesBase
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("person_role_id")]
        public int PersonRoleId { get; set; }

        [Column("military_rang_id")]
        public int? MilitaryRangId { get; set; }

        [Column("person_maturity_id")]
        public int? PersonMaturityId { get; set; }       

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(PersonRoleId))]
        public virtual PersonRole PersonRole { get; set; }

        [ForeignKey(nameof(PersonMaturityId))]
        public virtual PersonMaturity PersonMaturity { get; set; }

        [ForeignKey(nameof(MilitaryRangId))]
        public virtual MilitaryRang MilitaryRang { get; set; }

        public virtual ICollection<DocumentPersonAddress> Addresses { get; set; }

        public DocumentPerson()
        {
            Addresses = new HashSet<DocumentPersonAddress>();
        }
    }
}
