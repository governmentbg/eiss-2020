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
  [Table("dw_document_decision")]
  public class DWDocumentDecision : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public long Id { get; set; }

 
    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("decision_type_id")]
    [Display(Name = "Решение")]
    public int? DecisionTypeId { get; set; }

    [Column("decision_type_name")]
    [Display(Name = "Решение")]
    public string DecisionTypeName { get; set; }

    [Column("reg_number")]
    public string RegNumber { get; set; }

    [Column("reg_date")]
    public DateTime? RegDate { get; set; }

    [Column("out_document_id")]
    [Display(Name = "Документ за пренасочване")]
    public long? OutDocumentId { get; set; }

    [Column("user_decision_id")]
    public string UserDecisionId { get; set; }

    [Column("user_decision_name")]
    public string UserDecisionName { get; set; }

    [Column("description")]
    [Display(Name = "Забележка")]
    public string Description { get; set; }

    [Column("document_decision_state_id")]
    [Display(Name = "Статус")]

    public int DocumentDecisionStateId { get; set; }


    [Column("document_decision_state_name")]
    [Display(Name = "Статус")]

    public string DocumentDecisionStateName{ get; set; }

  }
}
