// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CasePersonSentencePunishmentCrimeVM
    {
        public int Id { get; set; }
        public string CaseCrimeLabel { get; set; }
        public string PersonRoleInCrimeLabel { get; set; }
        public string RecidiveTypeLabel { get; set; }
    }
}
