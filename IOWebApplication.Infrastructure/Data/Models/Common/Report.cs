// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
  /// <summary>
  /// Справки
  /// </summary>
  [Table("common_report")]
  public class Report
  {
    [Key]
    [Column("id")]

    public int Id { get; set; }

    [Column("court_type_id")]
    [Display(Name = "Тип")]
    public int CourtTypeId { get; set; }

    
    [Column("order_number")]
    [Display(Name = "Номер по ред")]
    public int OrderNumber { get; set; }

    [Column("report_name")]
    [Display(Name = "Справка")]
    public string ReportName { get; set; }
    [Column("report_path")]
    [Display(Name = "Адрес")]
    public string ReportPath { get; set; }


    [ForeignKey(nameof(CourtTypeId))]
    public virtual CourtType CourtType { get; set; }

    
  }
}
