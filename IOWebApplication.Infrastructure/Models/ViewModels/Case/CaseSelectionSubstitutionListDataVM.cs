// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    /// <summary>
    /// Модел за визуализация на избрани протоколи за случаен избор на заместващ в дело/заседание
    /// </summary>
    public class CaseSelectionSubstitutionListDataVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Данни за заседание
        /// </summary>
        public string SessionLabel { get; set; }

        /// <summary>
        /// Дата на протокол
        /// </summary>
        public DateTime SelectionDate { get; set; }

        /// <summary>
        /// Съдия за заместване
        /// </summary>
        public string SubstitutedLawUnitName { get; set; }

        /// <summary>
        /// Заместник
        /// </summary>
        public string SelectedLawUnitName { get; set; }

        /// <summary>
        /// Дата на запис
        /// </summary>
        public DateTime DateWrt { get; set; }
    }
}
