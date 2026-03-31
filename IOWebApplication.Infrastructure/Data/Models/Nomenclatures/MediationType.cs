// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид среща за медиация
    /// </summary>
    [Table("nom_mediation_type")]
    [Comment("Вид среща за медиация")]
    public class MediationType : BaseCommonNomenclature
    {
    }
}
