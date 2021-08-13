// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{

    /// <summary>
    /// Статус на призован чрез ДВ в списъка за призоваване ВКС
    /// </summary>
    [Table("nom_vks_notification_state")]
    public class VksNotificationState : BaseCommonNomenclature
    {
    }
}
