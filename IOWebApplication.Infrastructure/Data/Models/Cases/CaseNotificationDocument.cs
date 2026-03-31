// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    [Table("case_notification_document")]
    public class CaseNotificationDocument : UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("case_notification_id")]
        public int CaseNotificationId { get; set; }

        [Column("document_id")]
        public long DocumentId { get; set; }

        [Column("is_checked")]
        public bool IsChecked { get; set; }

        [ForeignKey(nameof(CaseNotificationId))]
        public virtual CaseNotification CaseNotification { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }
    }
    
}
