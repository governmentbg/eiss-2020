// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статуси на среща за медиация
    /// </summary>
    [Table("nom_mediation_state")]
    [Comment("Статуси на среща за медиация")]
    public class MediationState : BaseCommonNomenclature
    {
    }
}
