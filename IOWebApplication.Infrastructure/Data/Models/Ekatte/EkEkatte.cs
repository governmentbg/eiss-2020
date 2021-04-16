using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models
{
    [Table("ek_ekatte")]
    public class EkEkatte
    {
        [Column("ekatte")]
        public string Ekatte { get; set; }

        [Column("t_v_m")]
        public string TVM { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("oblast")]
        public string Oblast { get; set; }

        [Column("obstina")]
        public string Obstina { get; set; }

        [Column("kmetstvo")]
        public string Kmetstvo { get; set; }

        [Column("kind")]
        public string Kind { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("altitude")]
        public string Altitude { get; set; }

        [Column("document")]
        public string Document { get; set; }

        [Column("tsb")]
        public string Tsb { get; set; }

        [Column("abc")]
        public string Abc { get; set; }

        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("district_id")]
        public int? DistrictId { get; set; }

        [Column("country_id")]
        public int CountryId { get; set; }

        [Column("municipal_id")]
        public int? MunicipalId { get; set; }

        [Column("eispp_code")]
        public string EisppCode { get; set; } 

        [ForeignKey(nameof(DistrictId))]
        public EkDistrict District { get; set; }

        [ForeignKey(nameof(CountryId))]
        public EkCountry Country { get; set; }

        [ForeignKey(nameof(MunicipalId))]
        public EkMunincipality Munincipality { get; set; }

    }
}
