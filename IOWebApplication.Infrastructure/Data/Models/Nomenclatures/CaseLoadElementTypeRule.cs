// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Правила за елементи към група за натовареност по дела - основни дейности
    /// </summary>
    [Table("nom_case_load_element_type_rule")]
    public class CaseLoadElementTypeRule : BaseCommonNomenclature, IExpiredInfo
    {
        [Column("case_load_element_type_id")]
        public int CaseLoadElementTypeId { get; set; }

        [Column("session_type_id")]
        [Display(Name = "Вид заседаниe")]
        public int? SessionTypeId { get; set; }

        [Column("session_result_id")]
        [Display(Name = "Резултат от заседанието")]
        public int? SessionResultId { get; set; }

        [Column("act_type_id")]
        [Display(Name = "Тип акт")]
        public int? ActTypeId { get; set; }

        [Column("is_create_motive")]
        [Display(Name = "Изготвяне на мотив")]
        public bool? IsCreateMotive { get; set; }

        [Column("is_create_case")]
        [Display(Name = "Образуване на дело")]
        public bool? IsCreateCase { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(SessionTypeId))]
        public virtual SessionType SessionType { get; set; }

        [ForeignKey(nameof(SessionResultId))]
        public virtual SessionResult SessionResult { get; set; }

        [ForeignKey(nameof(ActTypeId))]
        public virtual ActType ActType { get; set; }

        [ForeignKey(nameof(CaseLoadElementTypeId))]
        public virtual CaseLoadElementType CaseLoadElementType { get; set; }
    }
}
