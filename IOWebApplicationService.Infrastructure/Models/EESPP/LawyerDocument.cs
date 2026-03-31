// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Models.EESPP
{
    public class LawyerDocument
    {
        public string NotificationID { get; set; }

        public string LegalBasis { get; set; }

        public string AppointFileName1 { get; set; }

        public string AppointFile1 { get; set; }

        public bool IsApproved { get; set; }

    }
}
