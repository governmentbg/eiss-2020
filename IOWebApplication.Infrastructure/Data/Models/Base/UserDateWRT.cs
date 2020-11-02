// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Base
{
    public class UserDateWRT : IUserDateWRT
    {
        [Column("user_id")]
        public string UserId { get; set; }
        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [Column("date_transfered_dw")]
        public DateTime? DateTransferedDW { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

    }
}
