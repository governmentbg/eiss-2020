// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Свързани дела към среща за медиация
    /// </summary>
    [Table("mediation_case_session_link_case")]
    [Comment("Свързани дела към среща за медиация")]
    public class MediationCaseSessionLinkCase
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на среща за медиация
        /// </summary>
        [Column("court_id")]
        [Comment("Идентификатор на среща за медиация")]
        public int MediationCaseSessionId { get; set; }

        /// <summary>
        /// Идентификатор на свързано дело
        /// </summary>
        [Column("link_case_id")]
        [Comment("Идентификатор на свързано дело")]
        public int LinkCaseId { get; set; }

        /// <summary>
        /// Среща за медиация
        /// </summary>
        [ForeignKey(nameof(MediationCaseSessionId))]
        public virtual MediationCaseSession MediationCaseSession { get; set; }

        /// <summary>
        /// Свързано дело
        /// </summary>
        [ForeignKey(nameof(LinkCaseId))]
        public virtual Case Case { get; set; }
    }
}
