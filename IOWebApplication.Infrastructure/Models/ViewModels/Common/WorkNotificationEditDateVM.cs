// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Модел за коригиране нотификацията да излезе след определени дни
    /// </summary>
    public class WorkNotificationEditDateVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        /// След колко днни да се визуализира отново нотификцията
        /// </summary>
        [Display (Name = "След колко дни да се визуализира отново нотификацията")]
        public int AddDays { get; set; }
    }
}
