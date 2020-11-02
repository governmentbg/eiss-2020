// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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
  [Table("dw_case_selection_protocol_lawunit")]
  public class DWCaseSelectionProtocolLawunit : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }
    [Column("id")]
    public int Id { get; set; }

  

    [Column("case_id")]
    public int? CaseId { get; set; }

    [Column("case_selection_protokol_id")]
    public int CaseSelectionProtokolId { get; set; }

    /// <summary>
    /// true - когато е избран по групата на делото
    /// </summary>
    [Column("selected_from_case_group")]
    public bool SelectedFromCaseGroup { get; set; }

    [Column("case_group_id")]
    [Display(Name = "Основен вид делo")]
    public int? CaseGroupId { get; set; }

    [Column("case_group_name")]
    [Display(Name = "Основен вид делo")]
    public string CaseGroupName{ get; set; }

    /// <summary>
    /// id на избрания съдия/заседател
    /// </summary>
    [Column("lawunit_id")]
    public int LawUnitId { get; set; }
    [Column("lawunit_name")]
    public string LawUnitName{ get; set; }

    [Column("load_index")]
    public int LoadIndex { get; set; }

    [Column("case_count")]
    public int CaseCount { get; set; }

    [Column("state_id")]
    public int StateId { get; set; }
    [Column("state_name")]
    public string StateName { get; set; }

    [Column("description")]
    public string Description { get; set; }




  }
}
