// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за ъпдейт на акт
    /// </summary>
    public class ActDeclaredMonthVM
    {

        public int CourtId { get; set; }
        /// <summary>
        /// Идентификатор на акт
        /// </summary>
        public int ActId { get; set; }

        /// <summary>
        /// Дата на постановяване
        /// </summary>
        public DateTime ActDeclaredDate { get; set; }

        /// <summary>
        /// Начална дата на заседание
        /// </summary>
        public DateTime SessionDateFrom { get; set; }
    }
}
