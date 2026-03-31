// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за доказателства за електронна папка и ход на дело
    /// </summary>
    public class CaseEvidenceEFVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата на регистрация
        /// </summary>
        public DateTime DateAccept { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Тип доказателство
        /// </summary>
        public string EvidenceTypeLabel { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string EvidenceStateLabel { get; set; }
    }
}
