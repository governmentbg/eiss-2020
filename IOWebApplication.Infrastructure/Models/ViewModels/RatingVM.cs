// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел за оценка
    /// </summary>
    public class RatingVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име/описание
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Стойност
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Флаг дали да се вижда стойността
        /// </summary>
        public bool IsViewValue { get; set; }

        /// <summary>
        /// Колона за подреждане
        /// </summary>
        public int OrderNumber { get; set; }

        /// <summary>
        /// Флаг дали да е малка контролата
        /// </summary>
        public bool IsSmall { get; set; } = false;
    }
}
