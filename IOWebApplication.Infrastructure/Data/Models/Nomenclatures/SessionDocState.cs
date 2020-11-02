// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на документ в заседание: Неразгледан,разгледан,окончателно разгледан
    /// </summary>
    [Table("nom_session_doc_state")]
    public class SessionDocState : BaseCommonNomenclature
    {
       
    }
}
