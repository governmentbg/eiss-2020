// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Случайно разпределяне: Статус на лице:1-избран;2-участва;3-не участва;4-отвод и т.н.
    /// </summary>
    [Table("nom_selection_lawunit_state")]
    public class SelectionLawUnitState : BaseCommonNomenclature
    {
        
    }
}
