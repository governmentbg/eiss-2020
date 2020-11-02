// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionMeetingUserVM
    {
        public int Id { get; set; }
        public string SecretaryUserName { get; set; }
        public DateTime DateWrt { get; set; }
    }
}
