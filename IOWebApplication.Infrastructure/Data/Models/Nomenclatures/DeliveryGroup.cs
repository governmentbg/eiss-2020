// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Начин на подаване/изпращане: Призовкар,поща,куриеска фирма,имейл
    /// </summary>
    [Table("nom_delivery_group")]
    public class DeliveryGroup : BaseCommonNomenclature
    {
        
    }
}
