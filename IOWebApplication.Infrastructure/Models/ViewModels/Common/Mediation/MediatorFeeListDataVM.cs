// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за извличане на данни за ставки за заплащане на медиатори
    /// </summary>
    public class MediatorFeeListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Вид среща за медиация
        /// </summary>
        public string MediationTypeLabel { get; set; }

        /// <summary>
        /// Възнаграждение на час
        /// </summary>
        public string HourFee { get; set; }

        /// <summary>
        /// Възнаграждение на час в евро
        /// </summary>
        public string HourFeeEUR { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}
