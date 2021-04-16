using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Съдилища
    /// </summary>
    [Table("common_court")]
    public class Court : BaseCommonNomenclature
    {
        [Column("ecli_code")]
        [Display(Name = "ECLI идентификатор")]
        public string EcliCode { get; set; }
        
        [Column("court_type_id")]
        [Display(Name = "Тип")]
        public int CourtTypeId { get; set; }
        
        [Column("city_name")]
        [Display(Name = "Град")]
        public string CityName { get; set; }

        [Column("city_code")]
        public string CityCode { get; set; }

        [Column("address")]
        [Display(Name = "Адрес")]
        public string Address { get; set; }

        [Column("court_logo")]
        public string CourtLogo { get; set; }

        [Column("parent_court_id")]
        public int? ParentCourtId { get; set; }

        [Column("court_region_id")]
        [Display(Name = "Съдебeн район")]
        public int? CourtRegionId { get; set; }

        [Column("eispp_code")]
        public string EISPPCode { get; set; }

        [Column("phone_number")]
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; }

        [Column("email")]
        [Display(Name = "Електронна поща")]
        public string Email { get; set; }

        [Column("address_id")]
        public long? AddressId { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(ParentCourtId))]
        public virtual Court ParentCourt { get; set; }

        [ForeignKey(nameof(CourtRegionId))]
        public virtual CourtRegion CourtRegion { get; set; }

        [ForeignKey(nameof(AddressId))]
        public virtual Address CourtAddress { get; set; }
    }
}
