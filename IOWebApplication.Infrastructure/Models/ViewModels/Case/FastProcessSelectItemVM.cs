// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел в който е попълват данни от парите за визуализация в бланка
    /// </summary>
    public class FastProcessSelectItemVM
    {
        /// <summary>
        /// Име
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Дата от
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Дата до
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Обяснителна част
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Законна лихва
        /// </summary>
        public bool HasStatutoryinterest { get; set; }

        /// <summary>
        /// Законна лихва дата
        /// </summary>
        public DateTime? StatutoryinterestDate { get; set; }
    }
}
