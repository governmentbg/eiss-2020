using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplicationService.Infrastructure.Data.Models.Base
{
  /// <summary>
  /// Съдилища
  /// </summary>

  public class DWCourt
  {
    [Column("dw_count")]
    public int? DwCount { get; set; }
    [Column("court_id")]
    public int? CourtId { get; set; }

    [Column("court_name")]

    public string CourtName { get; set; }

    [Column("ecli_code")]
    [Display(Name = "ECLI идентификатор")]
    public string EcliCode { get; set; }

    [Column("court_type_id")]
    [Display(Name = "Тип")]
    public int? CourtTypeId { get; set; }
    [Column("court_type_name")]

    public string CourtTypeName { get; set; }

    [Column("parent_court_id")]
    public int? ParentCourtId { get; set; }

    [Column("parent_court_name")]
    public string ParentCourtName { get; set; }

    [Column("court_region_id")]
    [Display(Name = "Съдебeн район")]
    public int? CourtRegionId { get; set; }
    [Column("court_region_name")]
    [Display(Name = "Съдебeн район")]
    public string CourtRegionName { get; set; }

    [Column("eispp_code")]
    public string EISPPCode { get; set; }




    [Column("city_name")]
    [Display(Name = "Град")]
    public string CityName { get; set; }

    [Column("city_code")]
    public string CityCode { get; set; }

    [Column("case_instance_id")]
    public int? CaseInstanceId { get; set; }
    [Column("case_instance_name")]
    public string CaseInstanceName { get; set; }
    [Column("case_instance_code")]
    public string CaseInstanceCode { get; set; }

    public DWCourt()
      {DwCount=1;
  }
  }
}
