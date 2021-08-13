// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    [Table("nom_notification_act_ispn_reason")]
    public class NotificationIspnReason
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }
        
        [Column("label")]
        public string Label { get; set; }
     
        [Column("accomply")]
        public string Accomply { get; set; }

        [Column("meeting_type")]
        public string MeetingType  { get; set; }

        [Column("meeting_agenda")]
        public string MeetingAgenda { get; set; }
    }
}

