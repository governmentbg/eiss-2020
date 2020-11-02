// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основен вид дело: Наказателни,граждански,търговски, фирмени и др.
    /// </summary>
    [Table("nom_case_group")]
    public class CaseGroup : BaseCommonNomenclature
    {
        public virtual ICollection<CaseType> CaseTypes { get; set; }

        public CaseGroup()
        {
            CaseTypes = new HashSet<CaseType>();
        }
    }
}
