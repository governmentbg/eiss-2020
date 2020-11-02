// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseLawUnitDismisalVM
    {
        public int Id { get; set; }
        public string CaseLawUnitName { get; set; }
        public string CaseLawUnitRole { get; set; }
        public int CaseLawUnitRoleId { get; set; }
        public string CaseLawUnitAkt { get; set; }
        public string DismisalTypeLabel { get; set; }
        public DateTime DismisalDate { get; set; }
        public string Description { get; set; }
    }
}
