// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Типове сесии в заседание:1-Сесия на заседание;2-Тайно заседание
    /// </summary>
    [Table("nom_session_meeting_type")]
    public class SessionMeetingType : BaseCommonNomenclature
    {
     
    }
}
