// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{

    /// <summary>
    /// Модел за извличане на данни за медиатори
    /// </summary>
    public class MediationCaseMediatorListDataVM : AppraisalVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на медиатор
        /// </summary>
        public string MediationMediatorName { get; set; }

        /// <summary>
        /// Вид избор на медиатора в делото
        /// </summary>
        public string MediationTypeChoiceMediatorLabel { get; set; }

        /// <summary>
        /// От
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}
