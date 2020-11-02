// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Резултат / Степен на уважаване на иска - към вид дело
    /// </summary>
    [Table("nom_act_complain_index_court_type_case_group")]
    public class ActComplainIndexCourtTypeCaseGroup
    {
        [Column("act_complain_index_id")]
        public int ActComplainIndexId { get; set; }

        [Column("court_type_id")]
        public int CourtTypeId { get; set; }
        
        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [ForeignKey(nameof(ActComplainIndexId))]
        public virtual ActComplainIndex ActComplainIndex { get; set; }

        [ForeignKey(nameof(CourtTypeId))]
        public virtual CourtType CourtType { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }
    }
}
