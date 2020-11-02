// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Известяване
    /// </summary>
    [Table("common_work_notification")]
    public class WorkNotification
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("work_notification_type_id")]
        [Display(Name = "Вид известие")]
        public int WorkNotificationTypeId { get; set; }

        [Column("title")]
        [Display(Name = "Инфо")]
        public string Title { get; set; }

        [Column("description")]
        [Display(Name = "Известие")]
        public string Description { get; set; }

        [Column("date_created")]
        [Display(Name = "Създадено")]
        public DateTime DateCreated { get; set; }

        [Column("date_read")]
        [Display(Name = "Видяно")]
        public DateTime? DateRead { get; set; }

        [Display(Name = "Изберете потребител")]
        [Column("user_id")]
        public string UserId { get; set; }
        
        [Column("from_court_id")]
        public int FromCourtId { get; set; }

        [Display(Name = "От потребител")]
        [Column("fromuser_id")]
        public string FromUserId { get; set; }

        [Column("link_label")]
        public string LinkLabel { get; set; }

        [Display(Name = "Срок")]
        [Column("case_deadline_id")]
        public int? CaseDeadlineId { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [NotMapped]
        public string SourceUrl { get; set; }


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }
        
        [ForeignKey(nameof(FromCourtId))]
        public virtual Court FromCourt { get; set; }
        [ForeignKey(nameof(WorkNotificationTypeId))]
        public virtual WorkNotificationType WorkNotificationType { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey(nameof(FromUserId))]
        public virtual ApplicationUser FromUser { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CaseDeadlineId))]
        public virtual CaseDeadline CaseDeadline { get; set; }

    }
}
