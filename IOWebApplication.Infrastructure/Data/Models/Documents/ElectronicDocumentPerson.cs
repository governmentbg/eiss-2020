using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Лица към електронно подаден документ
    /// </summary>
    [Table("electronic_document_person")]
    public class ElectronicDocumentPerson : PersonNamesBase
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("electronic_document_id")]
        public long ElectronicDocumentId { get; set; }

        [Column("person_role_id")]
        public int PersonRoleId { get; set; }

        [Column("person_gid")]
        [MaxLength(50)]
        public string PersonGid { get; set; }

        [Column("represents_gid")]
        [MaxLength(50)]
        public string RepresentsGid { get; set; }

        [ForeignKey(nameof(ElectronicDocumentId))]
        public virtual ElectronicDocument ElectronicDocument { get; set; }

        [ForeignKey(nameof(PersonRoleId))]
        public virtual PersonRole PersonRole { get; set; }

       
        public virtual ICollection<ElectronicDocumentPersonAddress> Addresses { get; set; }
    }
}
