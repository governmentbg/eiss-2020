// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Резултат / Степен на уважаване на иска - групиране    
    /// </summary>
    [Table("nom_act_result_grouping")]
    public class ActResultGrouping
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("act_result_id")]
        public int ActResultId { get; set; }

        [Column("from_case_instance_id")]
        public int FromCaseInstanceId { get; set; }

        [Column("to_case_instance_id")]
        public int ToCaseInstanceId { get; set; }

        [Column("case_group_id")]
        public int CaseGroupId { get; set; }

        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [ForeignKey(nameof(ActResultId))]
        public virtual ActResult ActResult { get; set; }

        [ForeignKey(nameof(FromCaseInstanceId))]
        public virtual CaseInstance FromCaseInstance { get; set; }

        [ForeignKey(nameof(ToCaseInstanceId))]
        public virtual CaseInstance ToCaseInstance { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }
    }
}
