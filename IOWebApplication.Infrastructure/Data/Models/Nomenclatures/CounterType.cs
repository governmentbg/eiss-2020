// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид брояч: По: документ, дело, документ в дело
    /// </summary>
    [Table("nom_counter_type")]
    public class CounterType : BaseCommonNomenclature
    {
    }
}
