// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид присъда: лишаване от свобода,глоба и т.н
    /// </summary>
    [Table("nom_sentence_type")]
    public class SentenceType : BaseCommonNomenclature
    {
        [Column("has_period")]
        public bool? HasPeriod { get; set; }
        [Column("has_money")]
        public bool? HasMoney { get; set; }
        [Column("has_probation")]
        public bool? HasProbation { get; set; }
        [Column("is_effective")]
        public bool? IsEffective { get; set; }
    }
}
