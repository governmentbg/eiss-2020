// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    [Table("case_session_act_period_notification")]
    [Comment("Таблица със срокове за нотификации към акт")]
    public class CaseSessionActPeriodNotification
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        [Comment("Идентификатор на записа")]
        [Column("id")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на акт
        /// </summary>
        [Comment("Идентификатор на акт")]
        [Column("case_session_act_id")]
        public int CaseSessionActId { get; set; }

        /// <summary>
        /// Идентификатор на тип нотификация
        /// </summary>
        [Comment("Идентификатор на тип нотификация")]
        [Column("work_notification_type_id")]
        public int WorkNotificationTypeId { get; set; }

        /// <summary>
        /// Да се създаде нотификация
        /// </summary>
        [Comment("Флаг дали да се създаде нотификация")]
        [Column("notification_on")]
        public bool? NotificationOn { get; set; }

        /// <summary>
        /// Нотификация след дни
        /// </summary>
        [Comment("Нотификация след дни")]
        [Column("notification_days")]
        public int? NotificationDays { get; set; }

        /// <summary>
        /// Нотификация след седмици
        /// </summary>
        [Comment("Нотификация след седмици")]
        [Column("notification_weeks")]
        public int? NotificationWeeks { get; set; }

        /// <summary>
        /// Нотификация след месеци
        /// </summary>
        [Comment("Нотификация след месеци")]
        [Column("notification_months")]
        public int? NotificationMonts { get; set; }

        /// <summary>
        /// Пояснение
        /// </summary>
        [Column("description")]
        public string Description { get; set; }

        /// <summary>
        /// Акт
        /// </summary>
        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        /// <summary>
        /// Тип нотификация
        /// </summary>
        [ForeignKey(nameof(WorkNotificationTypeId))]
        public virtual WorkNotificationType WorkNotificationType { get; set; }
    }
}
