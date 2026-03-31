// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{
    /// <summary>
    /// Модел за оценка в среща за медиация
    /// </summary>
    public class MediationCaseMediatorAppraisalVM
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
        /// Идентификатор на медиатор в дело
        /// </summary>
        [Display(Name = "Медиатор")]
        public int MediationCaseMediatorId { get; set; }

        /// <summary>
        /// Дата на оценка
        /// </summary>
        [Display(Name = "Дата на оценка")]
        public DateTime DateAppraisal { get; set; }

        /// <summary>
        /// Идентификатор на страна от делото в среща дала оценката
        /// </summary>
        [Display(Name = "Страна дала оценката")]
        public int? MediationCasePersonId { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        /// <summary>
        /// Тип оценка 1 - подробна / 2 - обобщена
        /// </summary>
        public int? TypeAppraisal { get; set; }

        /// <summary>
        /// Оценки
        /// </summary>
        public List<RatingVM> Appraisals { get; set; } = new();
    }
}
