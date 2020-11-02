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
    /// Групиране на точни видове документи - за справки и на всеки за каквото му трябва
    /// </summary>
    [Table("nom_document_type_grouping")]
    public class DocumentTypeGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("document_type_id")]
        public int DocumentTypeId { get; set; }

        [Column("document_type_group")]
        public int DocumentTypeGroup { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }
    }
}
