// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид интервал по дело:1-В процес,2-Временно преустановено,3-Възобновено
    /// </summary>
    [Table("nom_lifecycle_type")]
    public class LifecycleType : BaseCommonNomenclature
    {      

    }
}
