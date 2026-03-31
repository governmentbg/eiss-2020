// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Място на което се провеждат срещите за медиация
    /// </summary>
    [Table("nom_mediation_location")]
    [Comment("Място на което се провеждат срещите за медиация")]
    public class MediationLocation : BaseCommonNomenclature
    {
    }
}
