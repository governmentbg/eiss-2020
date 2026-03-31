// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case.Mediation
{

    /// <summary>
    /// Модел за извличане на данни за оценка в среща за медиация
    /// </summary>
    public class MediationCaseMediatorAppraisalListDataVM : AppraisalVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Медиатор в дело
        /// </summary>
        public string MediationCaseMediatorFullName { get; set; }

        /// <summary>
        /// Дата на оценка
        /// </summary>
        public DateTime DateAppraisal { get; set; }

        /// <summary>
        /// Страна дала оценката
        /// </summary>
        public string MediationCasePersonFullName { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Тип оценка
        /// </summary>
        public int TypeAppraisal { get; set; }

        /// <summary>
        /// Тип оценка
        /// </summary>
        public string TypeAppraisalText 
        {
            get
            {
                if (TypeAppraisal == NomenclatureConstants.MediationCaseMediatorAppraisalTypeConstants.Summary)
                    return "Обобщена";
                else
                    return "Подробна";
            }
        }
    }
}
