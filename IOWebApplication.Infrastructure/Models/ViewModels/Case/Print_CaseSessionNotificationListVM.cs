// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class Print_CaseSessionNotificationListVM
    {
        public string NameReport { get; set; }
        public string Title { get; set; }
        public string SessionTitle { get; set; }

        public virtual ICollection<CaseSessionNotificationListVM> NotificationLists { get; set; }
    }
}
