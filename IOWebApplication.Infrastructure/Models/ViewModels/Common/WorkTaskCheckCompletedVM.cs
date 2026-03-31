// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkTaskCheckCompletedVM
    {
        public bool HasTasks { get; set; }
        public bool HasUncompleteTasks { get; set; }
        public string LastUserIdCompleted { get; set; }
        public DateTime? LastDateCompleted { get; set; }
    }
}
