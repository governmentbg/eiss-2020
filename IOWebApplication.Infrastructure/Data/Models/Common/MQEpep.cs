// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_mq_epep")]
    public class MQEpep : UserDateWRT
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("mq_id")]
        public string MQId { get; set; }

        [Column("integration_type_id")]
        public int? IntegrationTypeId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("parent_source_id")]
        public long? ParentSourceId { get; set; }

        [Column("target_class_name")]
        public string TargetClassName { get; set; }

        /// <summary>
        /// add,update
        /// </summary>
        [Column("method_name")]
        public string MethodName { get; set; }

        //UTF-8 encoded, json serialized object
        [Column("content")]
        public byte[] Content { get; set; }

        /// <summary>
        /// Резултат от заявка - Guid на създаден или актуализиран обект
        /// </summary>
        [Column("return_guid_id")]
        public string ReturnGuidId { get; set; }

        /// <summary>
        /// Дата на успешно изпращане
        /// </summary>
        [Column("date_transfered")]
        public DateTime? DateTransfered { get; set; }

        [Column("integration_state_id")]
        public int? IntegrationStateId { get; set; }

        [Column("error_count")]
        public int? ErrorCount { get; set; }

        [Column("error_description")]
        public string ErrorDescription { get; set; }

        [ForeignKey(nameof(IntegrationTypeId))]
        public virtual IntegrationType IntegrationType { get; set; }

        [ForeignKey(nameof(IntegrationStateId))]
        public virtual IntegrationState IntegrationState { get; set; }
    }
}
