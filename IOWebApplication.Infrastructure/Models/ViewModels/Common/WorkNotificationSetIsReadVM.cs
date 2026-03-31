// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Модел който сетва групово на нотификации, че са прочетени
    /// </summary>
    public class WorkNotificationSetIsReadVM
    {
        /// <summary>
        /// Идентификатори на известия
        /// </summary>
        public string NotificationIds { get; set; }

        /// <summary>
        /// Основание
        /// </summary>
        [Display(Name = "Основание")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public string Description { get; set; }
    }
}
