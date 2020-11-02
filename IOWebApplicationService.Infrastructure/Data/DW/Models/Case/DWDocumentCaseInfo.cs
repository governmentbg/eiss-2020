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
  [Table("dw_document_case_info")]
  public class DWDocumentCaseInfo : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

 
    [Column("id")]
    public long Id { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }


    [Column("case_id")]
    public int? CaseId { get; set; }

    [Column("case_group_id")]
    public int? CaseGroupId { get; set; }
    [Column("case_group_name")]
    public string CaseGroupName { get; set; }

    /// <summary>
    /// Кодирания 5-цифрен номер на делото
    /// </summary>
    [Column("case_short_number")]
    public string CaseShortNumber { get; set; }

    [Column("case_year")]
    public int? CaseYear { get; set; }

    /// <summary>
    /// Кодирания 14-цифрен номер на делото
    /// </summary>
    [Column("case_reg_number")]
    public string CaseRegNumber { get; set; }

    [Column("act_type_id")]
    public int? ActTypeId { get; set; }
    [Column("act_type_name")]
    public string ActTypeName { get; set; }

    [Column("law_act_number")]
    public string ActNumber { get; set; }
    [Column("law_act_date")]
    public DateTime? ActDate { get; set; }
    [Column("session_act_id")]
    public int? SessionActId { get; set; }

    [Column("description")]
    public string Description { get; set; }

  }
}
