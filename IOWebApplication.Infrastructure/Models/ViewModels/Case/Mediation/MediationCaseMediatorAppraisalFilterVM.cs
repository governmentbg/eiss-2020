// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Филтър за извличане на оценка в среща за медиация
    /// </summary>
    public class MediationCaseMediatorAppraisalFilterVM
    {
        /// <summary>
        /// Идентификатор на среща
        /// </summary>
        public int MediationCaseSessionId { get; set; }
    }
}
