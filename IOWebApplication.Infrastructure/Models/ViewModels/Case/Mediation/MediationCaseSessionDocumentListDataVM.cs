// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{

    /// <summary>
    /// Модел за извличане на данни за документи в среща за медиация
    /// </summary>
    public class MediationCaseSessionDocumentListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата на качване
        /// </summary>
        public DateTime? DateUpload { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
    }
}
