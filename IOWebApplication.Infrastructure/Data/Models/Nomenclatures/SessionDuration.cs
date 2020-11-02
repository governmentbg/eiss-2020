// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Очаквана продължителност на заседание.
    /// </summary>
    [Table("nom_session_duration")]
    public class SessionDuration 
    {
        [Key]
        [Column("minutes")]
        public int Minutes { get; set; }

        [Column("label")]
        public string Label { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
