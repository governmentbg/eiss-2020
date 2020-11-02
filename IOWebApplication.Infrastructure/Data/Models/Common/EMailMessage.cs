// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_email_message")]
    public class EMailMessage : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("email_address")]
        public string EmailAddress { get; set; }

        [Column("body")]
        public string Body { get; set; }

        [Column("email_message_state_id")]
        public int EMailMessageStateId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }
        
        [Column("date_send")]
        public DateTime? DateSend { get; set; }

        [ForeignKey(nameof(EMailMessageStateId))]
        public virtual EMailMessageState EMailMessageState { get; set; }

        public virtual ICollection<EMailFile> EMailFiles { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }
    }
}
