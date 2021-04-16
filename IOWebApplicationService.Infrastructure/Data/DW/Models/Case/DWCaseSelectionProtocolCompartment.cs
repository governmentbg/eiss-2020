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
  [Table("dw_case_selection_protocol_compartment")]
  public class DWCaseSelectionProtocolCompartment : DWUserDateWRT
  {

    [Key]
    [Column("dw_Id")]
    public int dw_Id { get; set; }

    [Column("id")]
    public int Id { get; set; }

    [Column("court_id")]
    public int? CourtId { get; set; }

    [Column("case_id")]
    public int? CaseId { get; set; }

    [Column("case_selection_protokol_id")]
    public int CaseSelectionProtokolId { get; set; }
    [Column("lawunit_id")]
    public int LawUnitId { get; set; }

    [Column("lawunit_name")]
    public string LawUnitName { get; set; }
  }
}
