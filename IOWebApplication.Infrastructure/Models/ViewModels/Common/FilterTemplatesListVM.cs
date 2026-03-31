// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Модел за извличане на данни за шаблони за филтри за справки
    /// </summary>
    public class FilterTemplatesListVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Вид шаблон
        /// </summary>
        public string FilterTemplateTypeLabel { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Активност на записа
        /// </summary>
        public string IsActiveLabel { get; set; }
    }
}
