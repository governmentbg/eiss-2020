using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Адреси към Лице към деловоден документ
    /// </summary>
    [Table("common_person_address")]
    public class PersonAddress
    {
        //[Key]
        //[Column("id")]
        //public long Id { get; set; }

        [Column("person_id")]
        public int PersonId { get; set; }

        [Column("address_type_id")]
        public int AddressTypeId { get; set; }

        [Column("address_id")]
        public long AddressId { get; set; }

        [ForeignKey(nameof(PersonId))]
        public virtual Person Person { get; set; }

        [ForeignKey(nameof(AddressTypeId))]
        public virtual AddressType AddressType { get; set; }

        [ForeignKey(nameof(AddressId))]
        public virtual Address Address { get; set; }
    }
}
