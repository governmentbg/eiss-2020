// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид избор на медиатор в дело
    /// </summary>
    [Table("nom_mediation_type_choice_mediator")]
    [Comment("Вид избор на медиатор в дело")]
    public class MediationTypeChoiceMediator : BaseCommonNomenclature
    {
    }
}
