// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{

    /// <summary>
    /// Модел за извличане на данни за резултати в среща за медиация
    /// </summary>
    public class MediationCaseSessionResultListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Резултат от среща
        /// </summary>
        public string MediationResultLabel { get; set; }

        /// <summary>
        /// Основание за резултат от среща
        /// </summary>
        public string MediationResultBaseLabel { get; set; }

        /// <summary>
        /// Дали е основен резултат
        /// </summary>
        public string IsMainText { get; set; }
    }
}
