// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IAccessControl
    {
        bool IsRead { get; set; }
        bool CanAccess { get; set; }
        bool CanChange { get; set; }
        bool CanChangeFull { get; set; }
        string CdnFileEditMode { get; }
    }
}
