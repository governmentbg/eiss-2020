// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    /// <summary>
    /// Данни за редакция/добавяне в група централизирано разпределение ГД
    /// </summary>
    public class CourtLawUnitGroupCCEditVM
    {
        /// <summary>
        /// Идентификатор на записа
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Kind на група
        /// </summary>
        public int CourtGroupKind { get; set; }

        /// <summary>
        /// Идентификатор на съд
        /// </summary>
        [Display(Name = "Съд")]
        public int CourtId { get; set; }

        /// <summary>
        /// Идентификатор на служител
        /// </summary>
        [Display(Name = "Съдия")]
        public int LawUnitId { get; set; }
        public int LawUnitHandId { get; set; }

        /// <summary>
        /// Валидност на записа от дата
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Валидност на записа до дата
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Състав
        /// </summary>
        [Display(Name = "Състав")]
        public int? CourtDepartmentId { get; set; }

        /// <summary>
        /// Пояснение за дата до
        /// </summary>
        [Display(Name = "Пояснение за дата до")]
        public string DateToDescription { get; set; }

        /// <summary>
        /// Натовареност
        /// </summary>
        [Display(Name = "Натовареност")]
        public int LoadIndex { get; set; }

        /// <summary>
        /// Първоначално състояние на натовареност
        /// </summary>
        public int LoadIndexOld { get; set; }

        /// <summary>
        /// Пояснение за промяна на натовареност
        /// </summary>
        [Display(Name = "Пояснение за промяна на натовареност")]
        public string LoadIndexDescription { get; set; }

        public string LoadIndexDescriptionOld { get; set; }
    }
}
