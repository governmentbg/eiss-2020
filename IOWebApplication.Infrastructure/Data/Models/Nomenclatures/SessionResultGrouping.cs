// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групиране на резултати от заседание - за справки и на всеки за каквото му трябва
    /// </summary>
    [Table("nom_session_result_grouping")]
    public class SessionResultGrouping
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("session_result_id")]
        public int SessionResultId { get; set; }

        [Column("session_result_group")]
        public int SessionResultGroup { get; set; }

        [ForeignKey(nameof(SessionResultId))]
        public virtual SessionResult SessionResult { get; set; }
    }
}
