// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// инициатор на извикването - дело, регистратура, справка външни системи
    /// </summary>
    [Table("nom_regix_request_type")]
    public class RegixRequestType : BaseCommonNomenclature
    {
    }
}
