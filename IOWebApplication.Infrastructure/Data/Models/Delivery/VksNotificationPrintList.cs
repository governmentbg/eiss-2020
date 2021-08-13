// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    [Table("vks_notification_print_list")]
    public class VksNotificationPrintList: UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int CaseSessionId { get; set; }

        [Column("vks_notification_header_id")]
        public int VksNotificationHeaderId { get; set; }
        
        [Column("check_row")]
        public bool CheckRow { get; set; }

        [ForeignKey(nameof(VksNotificationHeaderId))]
        public virtual VksNotificationHeader VksNotificationHeader { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }
    }
}