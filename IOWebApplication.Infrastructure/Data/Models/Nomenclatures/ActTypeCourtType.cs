// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове актове по вид съд/инстанция/основен вид документ
    /// </summary>
    [Table("nom_act_type_court_link")]
    public class ActTypeCourtLink
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("act_type_id")]
        public int ActTypeId { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }

        [Column("case_instance_id")]
        public int CaseInstanceId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(ActTypeId))]
        public virtual ActType ActType { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseInstanceId))]
        public virtual CaseInstance CaseInstance { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
