// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Движение на веществени доказателства по дело
    /// </summary>
    [Table("case_evidence_movement")]
    public class CaseEvidenceMovement : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_evidence_id")]
        public int CaseEvidenceId { get; set; }

        [Column("evidence_movement_type_id")]
        [Display(Name = "Вид разпоредително действие")]
        public int EvidenceMovementTypeId { get; set; }

        [Column("movement_date")]
        [Display(Name = "Дата на разпоредително действие")]
        public DateTime MovementDate { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("act_description")]
        [Display(Name = "Описание съдебен акт")]
        public string ActDescription { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Съдебен акт")]
        public int? CaseSessionActId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseEvidenceId))]
        public virtual CaseEvidence CaseEvidence { get; set; }

        [ForeignKey(nameof(EvidenceMovementTypeId))]
        public virtual EvidenceMovementType EvidenceMovementType { get; set; }
    }
}
