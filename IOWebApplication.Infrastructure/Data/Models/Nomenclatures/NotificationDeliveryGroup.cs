// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове призоваване: С призовкар, с ДВ, Кметство, с куриер
    /// </summary>
    [Table("nom_notification_delivery_group")]
    public class NotificationDeliveryGroup : BaseCommonNomenclature
    {
    }
}
