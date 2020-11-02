// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CheckListViewVM
    {
        public int CourtId { get; set; }
        public int ObjectId { get; set; }
        public bool ShowLogOperation { get; set; }
        public int? OtherId { get; set; }
        public string Label { get; set; }
        public string ButtonLabel { get; set; }
        public IList<CheckListVM> checkListVMs { get; set; }
    }
}
