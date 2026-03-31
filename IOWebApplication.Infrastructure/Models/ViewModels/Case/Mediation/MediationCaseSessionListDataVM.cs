// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{

    /// <summary>
    /// Модел за извличане на данни за срещи по дело
    /// </summary>
    public class MediationCaseSessionListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Вид среща: Информационна среща; Процедура по медиация.
        /// </summary>
        public string MediationTypeLabel { get; set; }

        /// <summary>
        /// Място на което се провежда
        /// </summary>
        public string MediationLocationLabel { get; set; }

        /// <summary>
        /// От
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Статус на срещата
        /// </summary>
        public string MediationStateLabel { get; set; }

        /// <summary>
        /// Продължителност на среща
        /// </summary>
        public string Duration
        {
            get
            {
                if (DateTo == null)
                    return string.Empty;

                TimeSpan timeSpan = (DateTo ?? DateTime.Now).Subtract(DateFrom);
                return string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
        }
    }
}
