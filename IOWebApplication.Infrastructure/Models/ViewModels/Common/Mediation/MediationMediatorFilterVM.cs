// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Филтър на медиатори
    /// </summary>
    public class MediationMediatorFilterVM
    {
        /// <summary>
        /// Име на медиатор
        /// </summary>
        [Display(Name = "Име")]
        public string MediatorName { get; set; }
    }
}
