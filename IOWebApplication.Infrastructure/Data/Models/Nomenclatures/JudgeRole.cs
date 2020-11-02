// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Роля на съдебен служител: съдия,член,заседател
    /// </summary>
    [Table("nom_judge_role")]
    public class JudgeRole : BaseCommonNomenclature
    {      

    }
}
