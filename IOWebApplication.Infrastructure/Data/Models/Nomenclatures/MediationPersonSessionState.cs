// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на лицето след срещата - присъства и т.н. за медиация
    /// </summary>
    [Table("nom_mediation_person_session_state")]
    [Comment("Статус на лицето след срещата - присъства и т.н. за медиация")]
    public class MediationPersonSessionState : BaseCommonNomenclature
    {
    }
}
