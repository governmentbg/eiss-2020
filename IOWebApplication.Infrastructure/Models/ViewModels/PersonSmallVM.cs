// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    /// <summary>
    /// Модел с данни за лице
    /// </summary>
    public class PersonSmallVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        public string Uic { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        public string FullName { get; set; }
    }
}
