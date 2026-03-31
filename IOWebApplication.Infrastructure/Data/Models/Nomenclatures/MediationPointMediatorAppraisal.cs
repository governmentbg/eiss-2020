// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точки за оценяване на медиатори за медиация
    /// </summary>
    [Table("nom_mediation_point_mediator_appraisal")]
    [Comment("Точки за оценяване на медиатори за медиация")]
    public class MediationPointMediatorAppraisal : BaseCommonNomenclature
    {
        /// <summary>
        /// Флаг дали е ред на който не се дава оценка
        /// </summary>
        [Column("without_appraisal")]
        [Comment("Флаг дали е ред на който не се дава оценка")]
        public bool WithoutAppraisal { get; set; }

        /// <summary>
        /// Тип оценка 1 - подробна / 2 - обобщена
        /// </summary>
        [Column("type_point")]
        [Comment("Тип оценка 1 - подробна / 2 - обобщена")]
        public int? TypePoint {  get; set; }

        /// <summary>
        /// Ако типа е 2, това е на каква оценка отговаря
        /// </summary>
        [Column("multiplication_value")]
        [Comment("Ако типа е 2, това е на каква оценка отговаря")]
        public int? MultiplicationValue { get; set; }
    }
}
