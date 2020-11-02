// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Двойки стойности за обезличаване на документи
    /// </summary>
    [Table("case_depersonalization_value")]
    public class CaseDepersonalizationValue : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }
        /// <summary>
        /// String to be replaced
        /// </summary>
        [Column("search_value")]
        public string SearchValue { get; set; }

        /// <summary>
        /// String to be used for replacement
        /// </summary>
        [Column("replace_value")]
        public string ReplaceValue { get; set; }

        /// <summary>
        /// Search is case insesitive
        /// </summary>
        [Column("is_case_insensitive")]
        public bool IsCaseInsensitive { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }       
    }
}
