// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Медиатори
    /// </summary>
    [Table("common_mediation_mediator")]
    [Comment("Медиатори")]
    public class MediationMediator : UserDateWRT, IExpiredInfo
    {
        /// <summary>
        /// Идентификатор на запис
        /// </summary>
        [Key]
        [Column("id")]
        [Comment("Идентификатор на запис")]
        public int Id { get; set; }

        /// <summary>
        /// Имена на медиатора
        /// </summary>
        [Column("name")]
        [Comment("Имена на медиатора")]
        public string Name { get; set; }

        /// <summary>
        /// Допълнителна квалификация
        /// </summary>
        [Column("additional_qualification")]
        [Comment("Допълнителна квалификация")]
        public string AdditionalQualification { get; set; }

        /// <summary>
        /// Основна професия
        /// </summary>
        [Column("main_profession")]
        [Comment("Основна професия")]
        public string MainProfession { get; set; }

        /// <summary>
        /// Практика в определена област на правото
        /// </summary>
        [Column("practice_specific_area_law_year")]
        [Comment("Практика в определена област на правото")]
        public int? PracticeSpecificAreaLawYear { get; set; }

        /// <summary>
        /// Опит на медиатора в медиация по опредени видове спорове
        /// </summary>
        [Column("experience_mediation")]
        [Comment("Опит на медиатора в медиация по опредени видове спорове")]
        public string ExperienceMediation { get; set; }

        /// <summary>
        /// От дата
        /// </summary>
        [Column("date_from")]
        [Comment("От дата")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// До дата
        /// </summary>
        [Column("date_to")]
        [Comment("До дата")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Дата на вписване
        /// </summary>
        [Column("date_entry")]
        [Comment("Дата на вписване")]
        public DateTime? DateEntry { get; set; }

        /// <summary>
        /// Образование
        /// </summary>
        [Column("education")]
        [Comment("Образование")]
        public string Education { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [Column("description")]
        [Comment("Забележка")]
        public string Description { get; set; }

        /// <summary>
        /// Дата на анулиране
        /// </summary>
        [Column("date_expired")]
        [Comment("Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        /// <summary>
        /// Идентификатор на потребител, който е анулирал записа
        /// </summary>
        [Column("user_expired_id")]
        [Comment("Идентификатор на потребител, който е анулирал записа")]
        public string UserExpiredId { get; set; }

        /// <summary>
        /// Причина за анулиране
        /// </summary>
        [Column("description_expired")]
        [Comment("Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        /// <summary>
        /// Потребител, който е анулирал записа
        /// </summary>
        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        /// <summary>
        /// Центрове които обслужва медиатора
        /// </summary>
        public virtual ICollection<MediationMediatorCenter> Centers { get; set; }
    }
}
