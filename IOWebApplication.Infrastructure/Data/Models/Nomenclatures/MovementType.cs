// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид местоположение на дело: към лице,към структура от организацията или към външна организация
    /// </summary>
    [Table("nom_movement_type")]
    public class MovementType : BaseCommonNomenclature
    {
    }
}
