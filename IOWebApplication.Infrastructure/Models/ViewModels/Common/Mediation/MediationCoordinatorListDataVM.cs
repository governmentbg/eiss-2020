// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за извличане на данни за координатор
    /// </summary>
    public class MediationCoordinatorListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на координатор
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Длъжност
        /// </summary>
        public string Position { get; set; }

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
