// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Филтър на центрове за медиация
    /// </summary>
    public class MediationCenterFilterVM
    {
        /// <summary>
        /// Име на център
        /// </summary>
        [Display(Name = "Име")]
        public string CenterName { get; set; }
    }
}
