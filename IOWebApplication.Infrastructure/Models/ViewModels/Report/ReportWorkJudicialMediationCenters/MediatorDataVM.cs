// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report.ReportWorkJudicialMediationCenters
{
    /// <summary>
    /// Модел носещ данни за медиатори за отчет за работата на съдебните центрове по медиация 
    /// </summary>
    public class MediatorDataVM
    {
        /// <summary>
        /// Идентификатор на медиатора
        /// </summary>
        public int MediatorId { get; set; }

        /// <summary>
        /// Имена на медиатора
        /// </summary>
        public string MediatorName { get; set; }

        /// <summary>
        /// Идентификатор на център
        /// </summary>
        public int MediationCenterId { get; set; }

        /// <summary>
        /// Име на центъра
        /// </summary>
        public string MediationCenterName { get; set; }

        /// <summary>
        /// Съдилища които обслужва този център
        /// </summary>
        public int[] CourtIds { get; set; }
    }
}
