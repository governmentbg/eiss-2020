using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models
{
    [Table("ek_districts")]
    public class EkDistrict
    {
        [Column("district_id")]
        [Key]
        public int DistrictId { get; set; }

        [Column("oblast")]
        public string Oblast { get; set; }

        [Column("ekatte")]
        public string Ekatte { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("region")]
        public string Region { get; set; }

        [Column("document")]
        public string Document { get; set; }

        [Column("abc")]
        public string Abc { get; set; }

        [Column("country_id")]
        public int CountryId { get; set; }

        [ForeignKey(nameof(CountryId))]
        public EkCountry Country { get; set; }


    }
}
