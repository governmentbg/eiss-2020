// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Резултати по заседание към делото
    /// </summary>
    [Table("case_session_result")]
    public class CaseSessionResult: IExpiredInfo
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_session_id")]
        public int CaseSessionId { get; set; }

        /// <summary>
        /// Резултат на заседание, отложено, обявен за решаване, спряно ит.н.
        /// </summary>
        [Column("session_result_id")]
        [Display(Name = "Резултат от заседанието")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете резултат от заседанието")]
        public int SessionResultId { get; set; }

        /// <summary>
        ///  Основание за резултат от заседание
        /// </summary>
        [Column("session_result_base_id")]
        [Display(Name = "Основание")]
        public int? SessionResultBaseId { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Column("is_active")]
        [Display(Name = "Активен резултат")]
        public bool IsActive { get; set; }

        [Column("is_main")]
        [Display(Name = "Основен резултат")]
        public bool IsMain { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(SessionResultId))]
        public virtual SessionResult SessionResult { get; set; }

        [ForeignKey(nameof(SessionResultBaseId))]
        public virtual SessionResultBase SessionResultBase { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
    }
}
