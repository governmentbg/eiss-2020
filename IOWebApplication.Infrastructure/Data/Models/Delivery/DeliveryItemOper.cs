// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    /// <summary>
    /// Деиствия/операции по Призовки/съобщения 
    /// </summary>
    [Table("delivery_item_oper")]
    public class DeliveryItemOper : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("delivery_item_id")]
        public int DeliveryItemId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }
        
        [Column("delivery_oper_id")]
        public int DeliveryOperId { get; set; }

        [Column("date_oper")]
        public DateTime DateOper { get; set; }

        [Column("delivery_area_id")]
        public int? DeliveryAreaId { get; set; }

        [Column("notification_state_id")]
        public int NotificationStateId { get; set; }
        
        [Column("long")]
        public string Long { get; set; }

        [Column("lat")]
        public string Lat { get; set; }

        [Column("lawunit_id")]
        public int? LawUnitId { get; set; }

        [Column("delivery_info")]
        [Display(Name = "Данни за уведомяване")]
        public string DeliveryInfo { get; set; }

        [Column("delivery_reason_id")]
        [Display(Name = "Причина")]
        public int? DeliveryReasonId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(NotificationStateId))]
        public virtual NotificationState NotificationState { get; set; }

        [ForeignKey(nameof(DeliveryOperId))]
        public virtual DeliveryOper DeliveryOper { get; set; }
        
        [ForeignKey(nameof(DeliveryAreaId))]
        public virtual DeliveryArea DeliveryArea { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(DeliveryItemId))]
        public virtual DeliveryItem DeliveryItem { get; set; }

        [ForeignKey(nameof(DeliveryReasonId))]
        public virtual DeliveryReason DeliveryReason { get; set; }
    }
}
