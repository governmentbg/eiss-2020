// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// ЕИСПП - Видове номенклатури
    /// </summary>
    [Table("nom_eispp_tbl")]
    public class EisppTbl
    {
        [Key]
        [Column("code")]
        public string Code { get; set; }

        [Column("label")]
        public string Label { get; set; }

        [Column("system_name")]
        public string SystemName { get; set; }

        [Column("standart_no")]
        public int? StandartNo { get; set; }
    }
}
