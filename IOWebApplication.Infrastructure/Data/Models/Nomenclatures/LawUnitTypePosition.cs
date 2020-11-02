// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Връзки вид лице към длъжност
    /// </summary>
    [Table("nom_law_unit_type_position")]
    public class LawUnitTypePosition
    {
        [Column("law_unit_type_id")]
        public int LawUnitTypeId { get; set; }

        [Column("position_id")]
        public int PositionId { get; set; }

        [ForeignKey(nameof(LawUnitTypeId))]
        public virtual LawUnitType LawUnitType { get; set; }

        [ForeignKey(nameof(PositionId))]
        public virtual LawUnitPosition LawUnitPosition { get; set; }
    }
}
