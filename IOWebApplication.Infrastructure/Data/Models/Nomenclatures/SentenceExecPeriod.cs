// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Срок за изпълнение на присъда: до 3 дневен, до 7 дневен, над 7 дневен
    /// </summary>
    [Table("nom_sentence_exec_period")]
    public class SentenceExecPeriod : BaseCommonNomenclature
    {
       
    }
}
