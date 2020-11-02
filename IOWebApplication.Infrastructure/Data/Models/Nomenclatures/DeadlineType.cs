// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Точен вид срокове
    /// </summary>
    [Table("nom_deadline_type")]
    public class DeadlineType : BaseCommonNomenclature
    {
        [Display(Name = "Срок в дни")]
        [Column("deadline_days")]
        public int? DeadlineDays { get; set; }

        [Display(Name = "Срок в работни дни")]
        [Column("deadline_working_days")]
        public int? DeadlineWorkingDays { get; set; }

        [Display(Name = "Срок в месеци")]
        [Column("deadline_months")]
        public int? DeadlineMonths { get; set; }

        [Display(Name = "Срок сложни дела в дни")]
        [Column("deadline_special_days")]
        public int? DeadlineSpecialDays { get; set; }
        [Display(Name = "Срок сложни дела")]

        [Column("deadline_special_working_days")]
        public int? DeadlineSpecialWorkingDays { get; set; }

        [Column("deadline_special_months")]
        public int? DeadlineSpecialMonths { get; set; }

        [Column("deadline_group_id")]
        public int DeadlineGroupId { get; set; }


        [ForeignKey(nameof(DeadlineGroupId))]
        public virtual DeadlineGroup DeadlineGroup { get; set; }
    }
}
