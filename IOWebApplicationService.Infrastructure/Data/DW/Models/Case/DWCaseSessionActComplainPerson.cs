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
  [Table("dw_case_session_act_complain_person")]
  public class DWCaseSessionActComplainPerson : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public int Id { get; set; }
    [Column("case_id")]
    public int? CaseId { get; set; }
    [Column("case_session_id")]
    public int? CaseSessionId { get; set; }

    [Column("case_session_act_complain_id")]
    public int CaseSessionActComplainId { get; set; }
    [Column("complain_case_person_id")]
    [Display(Name = "Жалбоподател")]
    public int CasePersonId { get; set; }
    [Column("complain_case_person_name")]
    [Display(Name = "Жалбоподател")]
    public String CasePersonName { get; set; }

    [Column("date_expired")]
    [Display(Name = "Дата на анулиране сесия")]
    public DateTime? DateExpired { get; set; }
    [Column("date_expired_str")]
    [Display(Name = "Дата на анулиране сесия")]
    public string DateExpiredStr { get; set; }

  }
}
