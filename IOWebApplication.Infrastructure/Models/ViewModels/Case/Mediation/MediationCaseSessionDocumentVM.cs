// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за документи в среща за медиация
    /// </summary>
    public class MediationCaseSessionDocumentVM
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
        /// Дата на качване
        /// </summary>
        [Display(Name = "Дата на качване")]
        public DateTime? DateUpload { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [Display(Name = "Описание")]
        public string Description { get; set; }
    }
}
