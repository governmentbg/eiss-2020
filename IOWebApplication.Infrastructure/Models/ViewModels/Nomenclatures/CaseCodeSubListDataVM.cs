// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    /// <summary>
    /// Модел за извличане на данни за подкод на шифър в дело
    /// </summary>
    public class CaseCodeSubListDataVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Шифър
        /// </summary>
        public string CaseCodeLabel { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Етикет
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Активен
        /// </summary>
        public string IsActiveText { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}
