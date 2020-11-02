// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на решение за документ
    /// </summary>
    [Table("nom_document_decision_state")]
    public class DocumentDecisionState : BaseCommonNomenclature
    {
    }
}
