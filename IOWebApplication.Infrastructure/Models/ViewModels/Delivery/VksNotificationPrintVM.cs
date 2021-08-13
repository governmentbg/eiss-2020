// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public  class VksNotificationPrintVM
    {
        [Display(Name = "За месец")]
        public int VksNotificationHeaderId { get; set; }
        [Display(Name = "Издание на държавен вестник")]
        public string PaperEdition { get; set; }
    }
}
