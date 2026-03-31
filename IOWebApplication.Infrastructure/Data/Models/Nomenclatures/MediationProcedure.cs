// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове процедури по медиация
    /// </summary>
    [Table("nom_mediation_procedure")]
    [Comment("Видове процедури по медиация")]
    public class MediationProcedure : BaseCommonNomenclature
    {
    }
}
