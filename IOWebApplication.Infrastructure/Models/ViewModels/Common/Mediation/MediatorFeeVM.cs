// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;
using Microsoft.EntityFrameworkCore;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за добавяне/рекация на ставки за заплащане на медиатори
    /// </summary>
    public class MediatorFeeVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на вид среща за медиация
        /// </summary>
        [Display(Name = "Вид среща за медиация")]
        public int MediationTypeId { get; set; }

        /// <summary>
        /// Възнаграждение на час
        /// </summary>
        [Display(Name = "Възнаграждение на час")]
        public decimal HourFee { get; set; }

        /// <summary>
        /// Възнаграждение на час в евро
        /// </summary>
        [Display(Name = "Възнаграждение на час в евро")]
        public decimal HourFeeEUR { get; set; }

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
