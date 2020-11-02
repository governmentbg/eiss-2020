// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Стутус на призовка/известие:За връчване, връчена, невръчена
    /// </summary>
    [Table("nom_notification_state")]
    public class NotificationState : BaseCommonNomenclature
    {
    }
}
