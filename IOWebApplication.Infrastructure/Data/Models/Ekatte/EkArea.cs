// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models
{
    [Table("ek_areas")]
    public class EkArea
    {
        [Column("aread_id")]
        [Key]
        public int AreadId { get; set; }

        [Column("region")]
        [Required]
        public string Region { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; }

        [Column("document")]
        public string Document { get; set; }

        [Column("abc")]
        public string Abc { get; set; }

        [Column("name_en")]
        public string NameEn { get; set; }
    }
}

