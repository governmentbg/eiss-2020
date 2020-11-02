// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Models.ViewModels
{
    public class DeliveryAccountVM
    {
        public string MobileUserId { get; set; }
        public int CourtId { get; set; }
        public string MobileToken { get; set; }

        public string ApiAddress { get; set; }
        public string CourtName { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string RegisterGuid { get; set; }
    }
}
