// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точни видове документи към документен регистър
    /// </summary>
    [Table("nom_document_register_types")]
    public class DocumentRegisterType
    {
        [Column("document_register_id")]
        public int DocumentRegisterId { get; set; }

        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [ForeignKey(nameof(DocumentRegisterId))]
        public virtual DocumentRegister DocumentRegister { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }
    }
}
