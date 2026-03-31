// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за навигация в дело за медиация
    /// </summary>
    public class MediationCaseNavigationVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CourtId { get; set; }

        /// <summary>
        /// Идентификатор на среща
        /// </summary>
        public int MediationCaseSessionId { get; set; }
    }
}
