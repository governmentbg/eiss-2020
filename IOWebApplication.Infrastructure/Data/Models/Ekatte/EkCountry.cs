using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models
{
    [Table("ek_countries")]
    public class EkCountry
    {
        [Key]
        [Column("country_id")]
        public int CountryId { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("eispp_code")]
        public string EISPPCode { get; set; }

        [Column("code_3")]
        public string Code3 { get; set; }

        [Column("code_number")]
        public string CodeNumber { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
