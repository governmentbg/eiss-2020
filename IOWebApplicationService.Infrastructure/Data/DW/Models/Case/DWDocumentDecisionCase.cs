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
  [Table("dw_document_decision_case")]
  public class DWDocumentDecisionCase : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public long Id { get; set; }

    [Column("document_decision_id")]
    public long DocumentDecisionId { get; set; }

    [Column("case_id")]
    [Display(Name = "Дело")]
    public int CaseId { get; set; }

    [Column("decision_type_id")]
    [Display(Name = "Решение")]
    public int? DecisionTypeId { get; set; }
    [Column("decision_type_name")]
    [Display(Name = "Решение")]
    public string DecisionTypeName { get; set; }

    [Column("description")]
    [Display(Name = "Забележка")]
    public string Description { get; set; }

  }
}
