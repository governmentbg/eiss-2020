// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове номера: Адреси в район за призовки (Четни/Нечетни/...)
    /// </summary>
    [Table("nom_delivery_number_type")]
    public class DeliveryNumberType : BaseCommonNomenclature
    {
    }
}
