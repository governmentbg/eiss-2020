// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_apikey")]
    public class ApiKey
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("app_key")]
        public string AppKey { get; set; }
        [Column("app_secret")]
        public string AppSecret { get; set; }

        [Column("valid_to")]
        public DateTime? ValidTo { get; set; }


        [Column("remark")]
        public string Remark { get; set; }

        [Column("token_key")]
        public string TokenKey { get; set; }

        [Column("token_key_date")]
        public DateTime? TokenKeyDate { get; set; }

        [Column("token_key_expire_in")]
        public DateTime? TokenKeyExpiresIn { get; set; }
    }
}
