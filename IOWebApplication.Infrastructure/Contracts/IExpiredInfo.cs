// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IExpiredInfo
    {
        //int Id { get; set; }
        DateTime? DateExpired { get; set; }

        string UserExpiredId { get; set; }

        string DescriptionExpired { get; set; }
    }
}
