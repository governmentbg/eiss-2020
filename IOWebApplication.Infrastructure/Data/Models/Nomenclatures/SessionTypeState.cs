// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статуси по Вид заседание
    /// </summary>
    [Table("nom_session_type_state")]
    public class SessionTypeState 
    {
        [Column("session_type_id")]
        public int SessionTypeId { get; set; }

        [Column("session_state_id")]
        public int SessionStateId { get; set; }

        [ForeignKey(nameof(SessionTypeId))]
        public virtual SessionType SessionType { get; set; }

        [ForeignKey(nameof(SessionStateId))]
        public virtual SessionState SessionState { get; set; }
    }
}
