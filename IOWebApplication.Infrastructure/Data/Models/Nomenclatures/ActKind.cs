// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Под вид на съдебен акт
    /// </summary>
    [Table("nom_act_kind")]
    public class ActKind : BaseCommonNomenclature
    {
        [Column("act_type_id")]
        public int ActTypeId { get; set; }

        [Column("blank_name")]
        public string BlankName { get; set; }

        [Column("must_select_related_act")]
        public bool? MustSelectRelatedAct { get; set; }

        [Column("process_type")]
        public string ProcessType { get; set; }

        [ForeignKey(nameof(ActTypeId))]
        public virtual ActType ActType { get; set; }
    }
}
