// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    /// <summary>
    /// Деактивирани потребители в епеп
    /// </summary>
    public class UserDeactivatedRegistration
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid UserRegistrationId { get; set; }


        /// <summary>
        /// Дата на последна промяна
        /// </summary>
        public DateTime ModifyDate { get; set; }
    }
}
