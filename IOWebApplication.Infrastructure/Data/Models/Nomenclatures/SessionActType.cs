// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Типове актове към групи съдебни актове - за номериране
    /// </summary>
    [Table("nom_session_act_type")]
    public class SessionActType
    {
        [Column("session_act_group_id")]
        public int SessionActGroupId { get; set; }

        [Column("act_type_id")]
        public int ActTypeId { get; set; }

        [ForeignKey(nameof(SessionActGroupId))]
        public virtual SessionActGroup SessionActGroup { get; set; }

        [ForeignKey(nameof(ActTypeId))]
        public virtual ActType ActType { get; set; }

       
    }
}
