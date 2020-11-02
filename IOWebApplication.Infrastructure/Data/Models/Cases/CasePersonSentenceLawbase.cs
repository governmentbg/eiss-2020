// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Текстове от НК към присъда
    /// </summary>
    [Table("case_person_sentence_lawbase")]
    public class CasePersonSentenceLawbase : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_person_sentence_id")]
        public int CasePersonSentenceId { get; set; }

        [Column("sentence_lawbase_id")]
        public int SentenceLawbaseId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonSentenceId))]
        public virtual CasePersonSentence CasePersonSentence { get; set; }

        [ForeignKey(nameof(SentenceLawbaseId))]
        public virtual SentenceLawbase SentenceLawbase { get; set; }
    }
}
