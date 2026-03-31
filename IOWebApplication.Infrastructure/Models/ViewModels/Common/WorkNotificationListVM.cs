// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Модел за извличане на данни за известия
    /// </summary>
    public class WorkNotificationListVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Дата на създаване на нотификацията
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Информация за нотификацията
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Дата на преглед на нотификацията
        /// </summary>
        public DateTime? DateRead { get; set; }

        /// <summary>
        /// Допълнителна информация за нотификацията
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Флаг че не е прочетена нотификация
        /// </summary>
        public bool IsUnRead { get; set; }

        /// <summary>
        /// Флаг дали може да се редакцтира
        /// </summary>
        public bool IsEdit { get; set; }

        /// <summary>
        /// Линк до елемента
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// Kind на нотификацията
        /// </summary>
        public int NotificationKind { get; set; } = 1;

        /// <summary>
        /// Идентификатор на тип нотификация
        /// </summary>
        public int WorkNotificationTypeId { get; set; }

        /// <summary>
        /// Има на тип нотификация
        /// </summary>
        public string WorkNotificationTypeLabel { get; set; }
    }
}
