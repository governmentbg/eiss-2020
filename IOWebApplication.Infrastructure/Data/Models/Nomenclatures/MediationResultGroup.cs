// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групи за резултат от срещи за медиация
    /// </summary>
    [Table("nom_mediation_result_group")]
    [Comment("Групи за резултат от срещи за медиация")]
    public class MediationResultGroup : BaseCommonNomenclature
    {
    }
}
