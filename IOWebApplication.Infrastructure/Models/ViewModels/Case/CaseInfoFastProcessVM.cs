// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за преглед на данни за бързо производство
    /// </summary>
    public class CaseInfoFastProcessVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CaseCourtId { get; set; }

        /// <summary>
        /// Идентификатор на заседание
        /// </summary>
        public int? CaseSessionId { get; set; }

        /// <summary>
        /// Тип на дело
        /// </summary>
        public string CaseTypeLabel { get; set; }

        /// <summary>
        /// Регистрационен номер на дело
        /// </summary>
        public string CaseRegNumber { get; set; }

        /// <summary>
        /// Дата на образуване на дело
        /// </summary>
        public DateTime CaseRegDate { get; set; }

        /// <summary>
        /// Година на дело
        /// </summary>
        public int CaseYear { get; set; }

        /// <summary>
        /// Идентификатор на нотификация
        /// </summary>
        public int? CaseNotificationId { get; set; }

        /// <summary>
        /// Идентификатор на акт
        /// </summary>
        public int? CaseSessionActId { get; set; }

        /// <summary>
        /// Флаг, дали да създаде нотификация
        /// </summary>
        public bool CaseSessionActNotificationOn { get; set; }

        /// <summary>
        /// Регистрационен номер на акт
        /// </summary>
        public string ActRegNumber { get; set; }

        /// <summary>
        /// Вид на акта
        /// </summary>
        public string ActTypeLabel { get; set; }

        /// <summary>
        /// Дата на обявяване на акта
        /// </summary>
        public DateTime? ActDeclaredDate { get; set; }

        /// <summary>
        /// Дата на влизане в сила
        /// </summary>
        public DateTime? ActInforcedDate { get; set; }

        /// <summary>
        /// Дата на документа
        /// </summary>
        public DateTime? DocumentDate { get; set; }

        /// <summary>
        /// Лице което е уведомено с призовка
        /// </summary>
        public string NotificationDeliveryPersonFullName { get; set; }

        /// <summary>
        /// Нотификация след дни
        /// </summary>
        public int? DaysFastProcess { get; set; }

        /// <summary>
        /// Нотификация след седмици
        /// </summary>
        public int? WeeksFastProcess { get; set; }

        /// <summary>
        /// Нотификация след месеци
        /// </summary>
        public int? MontsFastProcess { get; set; }

        /// <summary>
        /// Регистрационен номер на изпълнителен лист
        /// </summary>
        public string ExecListNumber { get; set; }

        /// <summary>
        /// Име на съд който разнася призовката
        /// </summary>
        public string NotificationToCourtLabel { get; set; }

        /// <summary>
        /// Дата на събитие
        /// </summary>
        public DateTime? EventDate { get; set; }

        /// <summary>
        /// Призовкар
        /// </summary>
        public int? NotificationLawUnitId { get; set; }

        /// <summary>
        /// Пояснение
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Данни за служител
        /// </summary>
        public CaseLawUnitSmallVM CaseLawUnitSmall { get; set; } = null;
    }
}
