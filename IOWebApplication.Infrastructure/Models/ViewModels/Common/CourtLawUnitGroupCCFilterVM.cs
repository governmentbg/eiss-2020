// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Модел за филтър на данни за Група Централизирано разпределение ГД
    /// </summary>
    public class CourtLawUnitGroupCCFilterVM
    {
        /// <summary>
        /// Kind на група
        /// </summary>
        public int CourtGroupKind { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        [Display(Name = "От съд")]
        public int? CourtId { get; set; }

        /// <summary>
        /// Име на съдия
        /// </summary>
        [Display(Name = "Име на съдия")]
        public string LawUnitName { get; set; }
    }
}
