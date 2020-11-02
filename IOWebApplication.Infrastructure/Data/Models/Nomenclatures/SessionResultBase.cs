// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основание за резултат от заседание
    /// </summary>
    [Table("nom_session_result_base")]
    public class SessionResultBase : BaseCommonNomenclature
    {
        [Column("session_result_group_id")]
        public int SessionResultGroupId { get; set; }

        [ForeignKey(nameof(SessionResultGroupId))]
        public virtual SessionResultGroup SessionResultGroup { get; set; }
    }
}
