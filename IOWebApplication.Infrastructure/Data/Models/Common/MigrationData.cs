// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_migration_data")]
    public class MigrationData
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("data_type")]
        public string DataType { get; set; }

        [Column("data")]
        public string Data { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("parent_code")]
        public string ParentCode { get; set; }

        [Column("migration_date")]
        public DateTime? MigrationDate { get; set; }

        [Column("message")]
        public string Message { get; set; }
    }
}
