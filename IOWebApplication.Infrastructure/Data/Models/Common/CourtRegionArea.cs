using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Data.Models.Base;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{  /// <summary>
   /// Съдебен район
   /// </summary>
    [Table("common_court_region_area")]
    public class CourtRegionArea : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_region_id")]
        public int CourtRegionId { get; set; }

        [Display(Name = "Област")]
        [Column("district_code")]
        //ekatte от EkDistrict
        public string DistrictCode { get; set; }

        [Display(Name = "Община")]
        [Column("municipality_code")]
        //ekatte от EkMunicipality
        public string MunicipalityCode { get; set; }

        [Display(Name = "Активен")]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey(nameof(CourtRegionId))]
        public virtual CourtRegion CourtRegion { get; set; }
    }
}
