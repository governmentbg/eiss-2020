// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Филтър за извличане на страни по делото за срещата
    /// </summary>
    public class MediationCasePersonFilterVM
    {
        /// <summary>
        /// Идентификатор на среща
        /// </summary>
        public int MediationCaseSessionId { get; set; }
    }
}
