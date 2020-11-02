// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class EpepUserAssignmentVM
    {
        public int Id { get; set; }
        public int EpepUserId { get; set; }
        public int CaseId { get; set; }
        public string CourtName { get; set; }
        public string CaseInfo { get; set; }
        public string SideInfo { get; set; }
    }
}
