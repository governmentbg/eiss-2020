// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за филтриране на пари на медиатори за проведена среща
    /// </summary>
    public class MediationObligationFilterVM
    {
        /// <summary>
        /// Идентификатор на среща
        /// </summary>
        public int MediationCaseSessionId { get; set; }
    }
}
