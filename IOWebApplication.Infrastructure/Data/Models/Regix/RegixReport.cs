// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Regix
{
    /// <summary>
    /// Справки за различните интеграции
    /// </summary>
    [Table("regix_report")]
    public class RegixReport : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("regix_type_id")]
        public int RegixTypeId { get; set; }

        [Column("request_data", TypeName = "jsonb")]
        public string RequestData { get; set; }

        [Column("response_data", TypeName = "jsonb")]
        public string ResponseData { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_session_act_id")]
        public int? CaseSessionActId { get; set; }

        [Column("document_id")]
        public long? DocumentId { get; set; }

        [Column("description")]
        public string Description { get; set; }


        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(RegixTypeId))]
        public virtual RegixType RegixType { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }
    }
}
