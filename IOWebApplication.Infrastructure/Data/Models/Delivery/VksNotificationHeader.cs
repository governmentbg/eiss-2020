// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    [Table("vks_notification_header")]
    public class VksNotificationHeader : UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("month")]
        public int Month { get; set; }
        [Display(Name = "Издание на държавен вестник")]
        [Column("paper_edition")]
        public string PaperEdition { get; set; }
    }
}
