// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид инстанция: 1-ва, 2-ра, 3-та
    /// </summary>
    [Table("nom_case_instance")]
    public class CaseInstance : BaseCommonNomenclature
    {
    }
}
