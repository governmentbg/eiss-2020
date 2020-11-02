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
    /// Срокове по дела
    /// </summary>
    [Table("case_deadline")]
    public class CaseDeadline : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("case_session_result_id ")]
        [Display(Name = "Резултат от заседание")]
        public int? CaseSessionResultId { get; set; }

        [Column("deadline_group_id")]
        public int DeadlineGroupId { get; set; }

        [Column("deadline_type_id")]
        public int DeadlineTypeId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("date_complete")]
        public DateTime? DateComplete { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }
        
        [Column("result_expired_id")]
        [Display(Name = "Резултат прекратяващ срока")]
        public int? ResultExpiredId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(DeadlineGroupId))]
        public virtual DeadlineGroup DeadlineGroup { get; set; }

        [ForeignKey(nameof(DeadlineTypeId))]
        public virtual DeadlineType DeadlineType { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CaseSessionResultId))]
        public virtual CaseSessionResult CaseSessionResult { get; set; }

        [ForeignKey(nameof(ResultExpiredId))]
        public virtual CaseSessionResult ResultExpired { get; set; }
    }
}
