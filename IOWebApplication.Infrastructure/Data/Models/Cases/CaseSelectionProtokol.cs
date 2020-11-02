// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
  /// <summary>
  /// Протокол за разпределяне
  /// </summary>
  [Table("case_selection_protokol")]
  public class CaseSelectionProtokol : UserDateWRT
  {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("court_id")]
    public int CourtId { get; set; }

    [Column("case_id")]
    public int CaseId { get; set; }

    /// <summary>
    /// Тип разпределение:1-съдия-докладчик,2-член,3-заседател,4-резервен заседател
    /// </summary>
    [Column("judge_role_id")]
    public int JudgeRoleId { get; set; }


    /// <summary>
    /// Начин на разпределение:1-автоматично,2-ръчно,3-по дежурство
    /// </summary>
    [Column("selection_mode_id")]
    public int SelectionModeId { get; set; }

    /// <summary>
    /// Избрано направление
    /// </summary>
    [Column("court_department_id")]
    public int? CourtDepartmentId { get; set; }

    /// <summary>
    /// Избрано дежурство
    /// </summary>
    [Column("court_duty_id")]
    public int? CourtDutyId { get; set; }

    /// <summary>
    /// Търсена специалност, за заседатели
    /// </summary>
    [Column("speciality_id")]
    public int? SpecialityId { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("selection_date")]
    public DateTime SelectionDate { get; set; }

    /// <summary>
    /// id на избрания съдия/заседател
    /// </summary>
    [Column("selected_lawunit_id")]
    public int? SelectedLawUnitId { get; set; }

    [Column("case_lawunit_dismisal_id")]
    public int? CaseLawUnitDismisalId { get; set; }


    [Column("selection_protokol_state_id")]
    public int SelectionProtokolStateId { get; set; }

    /// <summary>
    /// Да се включат и съдиите от състава на съдията-докладчик
    /// </summary>
    [Column("include_compartment_judges")]
    public bool IncludeCompartmentJudges { get; set; }
    [Column("compartment_id")]
    public int? CompartmentID { get; set; }
    [Column("compartment_name")]
    public string CompartmentName { get; set; }


    [ForeignKey(nameof(CourtId))]
    public virtual Court Court { get; set; }

    [ForeignKey(nameof(CaseId))]
    public virtual Case Case { get; set; }

    [ForeignKey(nameof(JudgeRoleId))]
    public virtual JudgeRole JudgeRole { get; set; }

    [ForeignKey(nameof(SelectionModeId))]
    public virtual SelectionMode SelectionMode { get; set; }

    [ForeignKey(nameof(CourtDepartmentId))]
    public virtual CourtDepartment CourtDepartment { get; set; }

    [ForeignKey(nameof(CourtDutyId))]
    public virtual CourtDuty CourtDuty { get; set; }

    [ForeignKey(nameof(SelectedLawUnitId))]
    public virtual LawUnit SelectedLawUnit { get; set; }

    [ForeignKey(nameof(SpecialityId))]
    public virtual Speciality Speciality { get; set; }

    [ForeignKey(nameof(SelectionProtokolStateId))]
    public virtual SelectionProtokolState SelectionProtokolState { get; set; }

    [ForeignKey(nameof(CaseLawUnitDismisalId))]
    public virtual CaseLawUnitDismisal CaseLawUnitDismisal { get; set; }

    public virtual ICollection<CaseSelectionProtokolLawUnit> LawUnits { get; set; }
    public virtual ICollection<CaseSelectionProtokolCompartment> CompartmentLawUnits { get; set; } = new List<CaseSelectionProtokolCompartment>();
  }
}
