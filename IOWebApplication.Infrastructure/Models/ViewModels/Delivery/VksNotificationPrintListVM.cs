// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class VksNotificationPrintListVM
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public int CaseSessionId { get; set; }

        public int JudicalCompositionId { get; set; }
        public string CaseLabel { get; set; }
        public string CaseFromLabel { get; set; }
        public string JudicalCompositionLabel { get; set; }
        public string JudicalCompositionLabelGr { get; set; }
        public int SessionTime { get; set; }
        public string SessionTimeLabel { get; set; }
        public string LeftSide { get; set; }
        public string RightSide { get; set; }

        public bool CheckRow { get; set; }
    }
}
