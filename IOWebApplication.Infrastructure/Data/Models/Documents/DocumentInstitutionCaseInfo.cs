// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Данни за свързани дела от други институции към деловоден документ
    /// </summary>
    [Table("document_institution_case_info")]
    public class DocumentInstitutionCaseInfo
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("institution_id")]
        public int InstitutionId { get; set; }

        [Column("institution_case_type_id")]
        public int? InstitutionCaseTypeId { get; set; }

        /// <summary>
        /// Номер на делото
        /// </summary>
        [Column("case_number")]
        public string CaseNumber { get; set; }

        [Column("case_year")]
        public int CaseYear { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [ForeignKey(nameof(InstitutionId))]
        public virtual Institution Institution { get; set; }

        [ForeignKey(nameof(InstitutionCaseTypeId))]
        public virtual InstitutionCaseType InstitutionCaseType { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

    }
}
