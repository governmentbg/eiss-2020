// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    /// <summary>
    /// Модел за справка нотификации
    /// </summary>
    public class WorkNotificationFatsProcessVM
    {
        /// <summary>
        /// Идентификатор на нотификацията
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Име на потребител (имена на съдия-докладчик/съдебен служител получил нотификация);
        /// </summary>
        public string UserFullName { get; set; }

        /// <summary>
        /// Вид нотификация (Вид на нотификацията)
        /// </summary>
        public string NotificationTypeLabel { get; set; }

        /// <summary>
        /// Дата на получаване (Дата на получаване на нотификацията)
        /// </summary>
        public DateTime? NotificationDate { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string CaseRegNumber { get; set; }

        /// <summary>
        /// Флаг дали е от един и същ съд с на потребителя
        /// </summary>
        public bool IsLinkCase { get; set; }

        /// <summary>
        /// Флаг че не е прочетена нотификация
        /// </summary>
        public bool IsUnRead { get; set; }

        /// <summary>
        /// Дата на изключване
        /// </summary>
        public DateTime? DateTurnOff { get; set; }

        /// <summary>
        /// Причина за изключване
        /// </summary>
        public string DescriptionTurnOff { get; set; }
    }

    /// <summary>
    /// Модел филтър за справка нотификации
    /// </summary>
    public class WorkNotificationFilterFatsProcessVM
    {
        /// <summary>
        /// Име на потребител (имена на съдия-докладчик/съдебен служител получил нотификация);
        /// </summary>
        [Display(Name = "Име на потребител")]
        public string UserFullName { get; set; }

        /// <summary>
        /// Вид нотификация (Вид на нотификацията)
        /// </summary>
        [Display(Name = "Вид нотификация")]
        public int? NotificationTypeId { get; set; }

        /// <summary>
        /// От дата на получаване (Дата на получаване на нотификацията)
        /// </summary>
        [Display(Name = "От дата на получаване")]
        public DateTime? NotificationDateFrom { get; set; }

        /// <summary>
        /// До дата на получаване (Дата на получаване на нотификацията)
        /// </summary>
        [Display(Name = "До дата на получаване")]
        public DateTime? NotificationDateTo { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        [Display(Name = "Номер на дело")]
        public string CaseRegNumber { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int? CaseId { get; set; }
    }
}
