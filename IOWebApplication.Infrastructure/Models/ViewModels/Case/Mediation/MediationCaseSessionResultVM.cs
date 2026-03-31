// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за резултати в среща за медиация
    /// </summary>
    public class MediationCaseSessionResultVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CourtId { get; set; }

        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на среща
        /// </summary>
        public int MediationCaseSessionId { get; set; }

        /// <summary>
        /// Идентификатор на резултат от среща
        /// </summary>
        [Display(Name = "Резултат")]
        public int MediationResultId { get; set; }

        /// <summary>
        /// Идентификатор на основание за резултат от среща
        /// </summary>
        [Display(Name = "Основание за резултат")]
        public int? MediationResultBaseId { get; set; }

        /// <summary>
        /// Флаг за основен резултат
        /// </summary>
        [Display(Name = "Oсновен резултат")]
        public bool IsMain { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [Display(Name = "Забележка")]
        public string Description { get; set; }
    }
}
