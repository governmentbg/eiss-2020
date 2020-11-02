// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class ExpiredInfoVM : IExpiredInfo
    {
        public int Id { get; set; }
        public long LongId { get; set; }
        public DateTime? DateExpired { get; set; }
        public string UserExpiredId { get; set; }
        [Display(Name = "Причина за премахването")]
        [Required(ErrorMessage ="Въведете {0}.")]
        public string DescriptionExpired { get; set; }

        public string ExpireSubmitUrl { get; set; }
        public string DialogTitle { get; set; }
        public string ReturnUrl { get; set; }
    }
}
