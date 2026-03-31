// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EISS
{
    public class EissProcessActDeclaredVM
    {
        public string LastUserIdCompleted { get; set; }

        public DateTime? LastDateCompleted { get; set; }
    }
}
