// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел с данни за потребител
    /// </summary>
    public class UserDataVM
    {
        /// <summary>
        /// Идентификатор на потребителя на лице
        /// </summary>
        public string LawUnitUserId { get; set; }

        /// <summary>
        /// Име на лице
        /// </summary>
        public string LawUnitName { get; set; }
    }
}
