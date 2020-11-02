// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Дела към едно решени
    /// </summary>
    [Table("document_decision_case")]
    public class DocumentDecisionCase
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("document_decision_id")]
        public long DocumentDecisionId { get; set; }

        [Column("case_id")]
        [Display(Name = "Дело")]
        public int CaseId { get; set; }

        [Column("decision_type_id")]
        [Display(Name = "Решение")]
        public int? DecisionTypeId { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [ForeignKey(nameof(DocumentDecisionId))]
        public virtual DocumentDecision DocumentDecision { get; set; }

        [ForeignKey(nameof(DecisionTypeId))]
        public virtual DecisionType DecisionType { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }
    }
}
