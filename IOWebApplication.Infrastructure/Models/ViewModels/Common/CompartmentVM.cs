﻿using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CompartmentVM
    {
        public int Id { get; set; }

        public string Label { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}
