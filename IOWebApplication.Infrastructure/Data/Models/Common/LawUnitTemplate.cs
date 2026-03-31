// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Contracts;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Шаблони по потребители
    /// </summary>
    [Table("common_lawunit_template")]
    public class LawUnitTemplate : UserDateWRT, IHaveId
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Display(Name = "Наименование")]
        [Column("label")]
        public string Label { get; set; }

        [Column("lawunit_id")]
        public int LawunitId { get; set; }

        [Column("case_group_id")]
        [Display(Name = "Основен вид дело")]
        public int? CaseGroupId { get; set; }

        [Column("act_type_id")]
        [Display(Name = "Вид акт")]
        public int? ActTypeId { get; set; }

        [Column("act_main")]
        [Display(Name = "Основна част")]
        [AllowHtml]
        public string ActMain { get; set; }

        [Column("act_dispositive")]
        [Display(Name = "Диспозитив")]
        [AllowHtml]
        public string ActDispositive { get; set; }

        [Display(Name = "Активен запис")]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey(nameof(LawunitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(ActTypeId))]
        public virtual ActType ActType { get; set; }
    }
}
