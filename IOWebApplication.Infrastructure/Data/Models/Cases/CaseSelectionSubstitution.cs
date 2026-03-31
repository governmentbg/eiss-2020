// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Избрани протоколи за случаен избор на заместващ в дело/заседание
    /// </summary>
    [Table("case_selection_substitution")]
    public class CaseSelectionSubstitution : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_selection_protokol_substitution_id")]
        public int CaseSelectionProtokolSubstitutionId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int CaseSessionId { get; set; }

        [ForeignKey(nameof(CaseSelectionProtokolSubstitutionId))]
        public virtual CaseSelectionProtokolSubstitution CaseSelectionProtokolSubstitution { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }
    }
}
