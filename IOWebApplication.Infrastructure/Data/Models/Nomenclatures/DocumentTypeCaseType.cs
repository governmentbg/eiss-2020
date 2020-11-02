// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точни видове дела по иницииращи точни видове документи
    /// </summary>
    [Table("nom_document_type_case_type")]
    public class DocumentTypeCaseType
    {
        [Column("case_type_id")]
        public int CaseTypeId { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }

        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }
    }
}
