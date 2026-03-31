// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    [Table("case_session_act_correction")]
    public class CaseSessionActCorrection
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_session_act_id")]
        public int CaseSessionActId { get; set; }

        [Column("corrected_act_id")]
        public int CorrectedActId { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(CorrectedActId))]
        public virtual CaseSessionAct CorrectedAct { get; set; }
    }
}
