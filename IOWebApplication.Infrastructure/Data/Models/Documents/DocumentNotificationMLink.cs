// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    [Table("document_notification_mlink")]
    public class DocumentNotificationMLink
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("document_id")]
        public long? DocumentId { get; set; }

        [Column("document_notification_id")]
        public int DocumentNotificationId { get; set; }

        [Column("document_person_id")]
        public long? DocumentPersonId { get; set; }

        [Column("document_resolution_id")]
        public long? DocumentResolutionId { get; set; }

        [Column("document_person_summoned_id")]
        public long? DocumentPersonSummonedId { get; set; }

        [Column("document_person_link_id")]
        public int? DocumentPersonLinkId { get; set; }

        [Column("person_name")]
        public string PersonSummonedName { get; set; }

        [Column("person_role")]
        public string PersonSummonedRole { get; set; }

        [Column("is_checked")]
        public bool IsChecked { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [NotMapped]
        [Display(Name = "Описане на връзката")]
        public string LinkLabel { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(DocumentResolutionId))]
        public virtual DocumentResolution DocumentResolution { get; set; }

        [ForeignKey(nameof(DocumentPersonSummonedId))]
        public virtual DocumentPerson DocumentPersonSummoned { get; set; }

        [ForeignKey(nameof(DocumentPersonId))]
        public virtual DocumentPerson DocumentPerson { get; set; }

        [ForeignKey(nameof(DocumentNotificationId))]
        public virtual DocumentNotification DocumentNotification { get; set; }
    }
}
