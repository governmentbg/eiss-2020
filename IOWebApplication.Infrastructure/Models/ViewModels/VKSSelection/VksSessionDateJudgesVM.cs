// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.VKSSelection
{
    public class VksSessionDateJudgesVM
    {
        public int CourtDepartmentId { get; set; }
        public bool IsPredsedatel { get; set; }
        public List<VksSessionJudgeVM> Judges { get; set; }
    }

    public class VksSessionJudgeVM
    {
        public int LawunitId { get; set; }
        public string LawunitUserId { get; set; }
    }
}
