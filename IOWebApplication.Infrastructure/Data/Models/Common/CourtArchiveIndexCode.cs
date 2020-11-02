// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Кодове по Индекси по съдилища,за архивиране
    /// </summary>
    [Table("common_court_archive_index_codes")]
    public class CourtArchiveIndexCode
    {
        [Column("court_archive_index_id")]
        public int CourtArchiveIndexId { get; set; }

        [Column("case_code_id")]
        public int CaseCodeId { get; set; }

        [Column("date_from")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtArchiveIndexId))]
        public virtual CourtArchiveIndex CourtArchiveIndex { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        public virtual CaseCode CaseCode { get; set; }

    }
}
