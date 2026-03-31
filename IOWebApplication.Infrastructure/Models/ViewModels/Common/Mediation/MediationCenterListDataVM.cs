// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за извличане на данни за центрове за медиация
    /// </summary>
    public class MediationCenterListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Горно ниво
        /// </summary>
        public string ParentText { get; set; }

        /// <summary>
        /// Име на центъра
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string AddressText { get; set; }

        /// <summary>
        /// Контактни данни
        /// </summary>
        public string ContactDetails { get; set; }

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
