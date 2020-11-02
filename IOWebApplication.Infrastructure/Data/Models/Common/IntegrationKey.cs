// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Външни стойности за интеграция в други системи
    /// </summary>
    [Table("common_integration_keys")]
    public class IntegrationKey : UserDateWRT
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
       
        [Column("integration_type_id")]
        public int IntegrationTypeId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("outer_code")]
        public string OuterCode { get; set; }

        [ForeignKey(nameof(IntegrationTypeId))]
        public virtual IntegrationType IntegrationType { get; set; }
    }
}
