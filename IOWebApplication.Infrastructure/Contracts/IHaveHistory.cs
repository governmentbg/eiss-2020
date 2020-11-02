// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IHaveHistory<IHistory>
    {
        ICollection<IHistory> History { get; set; }
    }
}
