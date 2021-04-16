using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplicationService.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
  [Table("dw_case_selection_protocol")]
  public class DWCaseSelectionProtocol : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public int Id { get; set; }

    [Column("case_id")]
    public int CaseId { get; set; }

    /// <summary>
    /// Тип разпределение:1-съдия-докладчик,2-член,3-заседател,4-резервен заседател
    /// </summary>
    [Column("judge_role_id")]
    public int JudgeRoleId { get; set; }
    [Column("judge_role_name")]
    public string JudgeRoleName { get; set; }

    /// <summary>
    /// Начин на разпределение:1-автоматично,2-ръчно,3-по дежурство
    /// </summary>
    [Column("selection_mode_id")]
    public int SelectionModeId { get; set; }
    [Column("selection_mode_name")]
    public string SelectionModeName { get; set; }

    /// <summary>
    /// Избрано направление
    /// </summary>
   

    /// <summary>
    /// Избрано дежурство
    /// </summary>
    [Column("court_duty_id")]
    public int? CourtDutyId { get; set; }

    [Column("court_duty_name")]
    public string CourtDutyName { get; set; }

    [Column("court_group_id")]
    public int? CourtGroupId { get; set; }

    [Column("court_group_name")]
    public string CourtGroupName { get; set; }

    /// <summary>
    /// Търсена специалност, за заседатели
    /// </summary>
    [Column("speciality_id")]
    public int? SpecialityId { get; set; }
    /// Търсена специалност, за заседатели
    /// </summary>
    [Column("speciality_name")]
    public string SpecialityName { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("selection_date")]
    public DateTime SelectionDate { get; set; }

    /// <summary>
    /// id на избрания съдия/заседател
    /// </summary>
    [Column("selected_lawunit_id")]
    public int? SelectedLawUnitId { get; set; }

    [Column("selected_lawunit_name")]
    public string SelectedLawUnitName { get; set; }

    [Column("case_lawunit_dismisal_id")]
    public int? CaseLawUnitDismisalId { get; set; }


    [Column("selection_protokol_state_id")]
    public int SelectionProtokolStateId { get; set; }

    [Column("selection_protokol_state_name")]
    public string SelectionProtokolStateName{ get; set; }

    /// <summary>
    /// Да се включат и съдиите от състава на съдията-докладчик
    /// </summary>
    [Column("include_compartment_judges")]
    public bool IncludeCompartmentJudges { get; set; }
    [Column("compartment_id")]
    public int? CompartmentID { get; set; }
    [Column("compartment_name")]
    public string CompartmentName { get; set; }


  }
}
