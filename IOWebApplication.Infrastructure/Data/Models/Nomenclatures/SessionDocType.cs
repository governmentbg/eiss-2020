// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид на документ внесен в заседание: Възражение; Изложение; Експертиза/заключение на вещо лице; Становище; Писмени бележки; Молба; Друг документ
    /// </summary>
    [Table("nom_session_doc_type")]
    public class SessionDocType : BaseCommonNomenclature
    {
       
    }
}
