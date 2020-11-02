// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид на правораздаващо лице: 1-съдия,2-заседател,3-прокурор,4-вещо лице
    /// </summary>
    [Table("nom_law_unit_type")]
    public class LawUnitType : BaseCommonNomenclature
    {      

    }
}
