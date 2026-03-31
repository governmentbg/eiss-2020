// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using static IOWebApplication.Infrastructure.Constants.NomenclatureConstants;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common.Mediation
{
    /// <summary>
    /// Модел за добавяне/рекация на координатор
    /// </summary>
    public class MediationCoordinatorVM
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на координатор
        /// </summary>
        [Display(Name = "Координатор")]
        public int LawUnitId { get; set; }

        /// <summary>
        /// Длъжност
        /// </summary>
        [Display(Name = "Позиция")]
        public string Position { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Забележка")]
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

        /// <summary>
        /// Идентификатори на центрове
        /// </summary>
        [Display(Name = "Центрове")]
        public string[] CenterIds { get; set; }

        /// <summary>
        /// Стринг с идентификатори на центрове, разделени със запетая
        /// </summary>
        public string StringCenterIds
        {
            get
            {
                if ((CenterIds != null) && CenterIds.Length > 0)
                    return string.Join(",", CenterIds);
                else
                    return string.Empty;
            }
        }
    }
}
