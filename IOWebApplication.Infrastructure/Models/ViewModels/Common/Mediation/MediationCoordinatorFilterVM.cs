// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Филтър на Координатори
    /// </summary>
    public class MediationCoordinatorFilterVM
    {
        /// <summary>
        /// Име на кординатор
        /// </summary>
        [Display(Name = "Име")]
        public string CoordinatorName { get; set; }
    }
}
