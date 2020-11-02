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
  /// Протокол за разпределяне-заключване
  /// </summary>
  [Table("case_selection_protokol_lock")]
  public class CaseSelectionProtokolLock 
  {
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("court_group_id")]
    public int? CourtGroupId { get; set; }
    /// <summary>
    /// Избрано дежурство
    /// </summary>
    [Column("court_duty_id")]
    public int? CourtDutyId { get; set; }

    [Column("date")]

    public DateTime Date { get; set; }

    [Column("date_finish")]

    public DateTime? DateFinish { get; set; }
  }
}
