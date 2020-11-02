// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
  /// <summary>
  /// извиквания на справки
  /// </summary>
  [Table("common_report_request")]
  public class ReportRequest
  {
    [Key]
    [Column("id")]
    public string Id { get; set; }

    [Column("report_id")]
    public int ReportId { get; set; }

    [Column("court_list")]
    public string CourtList { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
    [Column("date_wrt")]
    public DateTime DateWrt { get; set; }

    [Column("validation_guid")]
    public string ValidationGuid { get; set; }
    [Column("date_get_report")]
    public DateTime? DateGetReport { get; set; }


    [ForeignKey(nameof(ReportId))]
    public virtual Report Report { get; set; }

  
  
  }
}
