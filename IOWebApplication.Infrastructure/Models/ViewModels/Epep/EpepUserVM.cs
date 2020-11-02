// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class EpepUserVM
    {
        public int Id { get; set; }
        public string UserTypeName { get; set; }
        public int EpepUserTypeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string LawyerNumber { get; set; }
    }
}
