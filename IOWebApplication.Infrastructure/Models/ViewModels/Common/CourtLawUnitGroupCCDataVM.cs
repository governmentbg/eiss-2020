// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Данни за Група Централизирано разпределение ГД
    /// </summary>
    public class CourtLawUnitGroupCCDataVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        public int CourtId { get; set; }

        /// <summary>
        /// Име на съд
        /// </summary>
        public string CourtName { get; set; }

        /// <summary>
        /// Идентификатор на служител
        /// </summary>
        public int LawUnitId { get; set; }

        /// <summary>
        /// Име на служител
        /// </summary>
        public string LawUnitName { get; set; }

        /// <summary>
        /// Валидност на записа от дата
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Валидност на записа до дата
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Флаг за редакция
        /// </summary>
        public bool IsEdit { get; set; }

        /// <summary>
        /// Състав
        /// </summary>
        public string CourtDepartmentLabel { get; set; }

        /// <summary>
        /// Натовареност
        /// </summary>
        public int LoadIndex { get; set; }
    }
}
