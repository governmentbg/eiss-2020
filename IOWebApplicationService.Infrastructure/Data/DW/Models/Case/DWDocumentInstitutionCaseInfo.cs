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
  [Table("dw_document_institution_case_info")]
  public class DWDocumentInstitutionCaseInfo : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public long Id { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("institution_id")]
    public int InstitutionId { get; set; }
    [Column("institution_name")]
    public string InstitutionName { get; set; }

    [Column("institution_case_type_id")]
    public int? InstitutionCaseTypeId { get; set; }
    [Column("institution_case_type_name")]
    public string InstitutionCaseTypeName { get; set; }

    /// <summary>
    /// Номер на делото
    /// </summary>
    [Column("case_number")]
    public string CaseNumber { get; set; }

    [Column("case_year")]
    public int CaseYear { get; set; }

    [Column("description")]
    public string Description { get; set; }


  }
}
