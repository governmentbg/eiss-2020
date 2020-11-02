// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    /// <summary>
    /// Посещение отразено през мобилния призовкар
    /// </summary>
    [Table("delivery_item_visit_mobile")]
    public class DeliveryItemVisitMobile
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("delivery_item_id")]
        public int DeliveryItemId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("visit_count")]
        public int VisitCount { get; set; }

        [Column("delivery_oper_id")]
        public int DeliveryOperId { get; set; }

        [Column("delivery_reason_id")]
        public int DeliveryReasonId { get; set; }

        [Column("date_oper")]
        public DateTime DateOper { get; set; }

        [Column("notification_state_id")]
        public int NotificationStateId { get; set; }

        [Column("long")]
        public string Long { get; set; }

        [Column("lat")]
        public string Lat { get; set; }

        [Column("lawunit_id")]
        public int LawUnitId { get; set; }

        [Column("date_api")]
        public DateTime? DateAPI { get; set; }
        
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("is_ok")]
        public bool IsOK { get; set; }
        
        [Column("error")]
        public string Error { get; set; }
        
        [Column("delivery_info")]
        public string DeliveryInfo { get; set; }

        [Column("delivery_uuid")]
        public string DeliveryUUID { get; set; }

    }
}
