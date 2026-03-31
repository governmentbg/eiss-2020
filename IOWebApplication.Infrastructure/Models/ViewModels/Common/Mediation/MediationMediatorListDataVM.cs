// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за извличане на данни за медиатори
    /// </summary>
    public class MediationMediatorListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на центъра
        /// </summary>
        public string Name { get; set; }

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
