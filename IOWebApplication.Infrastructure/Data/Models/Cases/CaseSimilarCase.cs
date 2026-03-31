// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Таблица със сходни дела за заповедно производство
    /// </summary>
    [Table("case_similar_case")]
    [Comment("Таблица със сходни дела за заповедно производство")]
    public class CaseSimilarCase
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        [Column("case_id")]
        [Comment("Идентификатор на дело")]
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на сходно дело
        /// </summary>
        [Column("similar_case_id")]
        [Comment("Идентификатор на дело")]
        public int SimilarCaseId { get; set; }

        /// <summary>
        /// Дело
        /// </summary>
        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        /// <summary>
        /// Дело
        /// </summary>
        [ForeignKey(nameof(SimilarCaseId))]
        public virtual Case SimilarCase { get; set; }
    }
}
