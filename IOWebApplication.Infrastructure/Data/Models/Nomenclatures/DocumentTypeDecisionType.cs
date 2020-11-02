// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Линкваща таблица за DocumentType и DecisionType
    /// </summary>
    [Table("nom_document_type_decision_type")]
    public class DocumentTypeDecisionType
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [Column("decision_type_id")]
        public int DecisionTypeId { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }

        [ForeignKey(nameof(DecisionTypeId))]
        public virtual DecisionType DecisionType { get; set; }
    }
}
