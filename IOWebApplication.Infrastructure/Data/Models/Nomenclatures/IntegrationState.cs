// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на заявка за трансфер
    /// </summary>
    [Table("nom_integration_state")]
    public class IntegrationState : BaseCommonNomenclature
    {
    }
}
