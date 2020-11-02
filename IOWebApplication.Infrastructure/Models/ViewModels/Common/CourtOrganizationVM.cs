// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtOrganizationVM
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string CourtLabel { get; set; }
        public string ParentLabel { get; set; }
        public string Label { get; set; }
        public string OrganizationLevelLabel { get; set; }
    }
}
