// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures
{
    /// <summary>
    /// Модел за добавяне/рекация на подкод на шифър в дело
    /// </summary>
    public class CaseCodeSubVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на шифър
        /// </summary>
        [Display(Name = "Основен шифър")]
        public int? CaseCodeId { get; set; }

        /// <summary>
        /// Номер по ред
        /// </summary>
        [Display(Name = "Номер по ред")]
        public int OrderNumber { get; set; }

        /// <summary>
        /// Код на подшифър
        /// </summary>
        [Display(Name = "Код на подшифър")]
        public string Code { get; set; }

        /// <summary>
        /// Подшифър
        /// </summary>
        [Display(Name = "Наименование подшифър")]
        public string Label { get; set; }

        /// <summary>
        /// Активен
        /// </summary>
        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Законов текст")]
        public string Description { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
    }
}
