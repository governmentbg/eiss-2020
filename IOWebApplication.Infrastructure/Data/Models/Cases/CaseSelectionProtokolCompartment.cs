// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Протокол за разпределяне: Списък с съдии от състава на съдията-докладчик - ако е избрано
    /// </summary>
    [Table("case_selection_protokol_compartment")]
    public class CaseSelectionProtokolCompartment : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_selection_protokol_id")]
        public int CaseSelectionProtokolId { get; set; }

        /// <summary>
        /// id на избрания съдия/заседател
        /// </summary>
        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSelectionProtokolId))]
        public virtual CaseSelectionProtokol CaseSelectionProtokol { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }
    }
}
