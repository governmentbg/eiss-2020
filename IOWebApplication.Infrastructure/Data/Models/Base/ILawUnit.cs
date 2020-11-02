// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Base
{
   public interface ILawUnit
    {
        int LawUnitId { get; set; }

        LawUnit LawUnit { get; set; }
    }
}
