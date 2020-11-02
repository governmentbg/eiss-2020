// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models
{
    [Table("ek_sobr")]
    public class EkSobr
    {
        [Column("ekatte")]
        public string Ekatte { get; set; }

        [Column("kind")]
        public string Kind { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("area1")]
        public string Area1 { get; set; }

        [Column("area2")]
        public string Area2 { get; set; }

        [Column("document")]
        public string Document { get; set; }

        [Column("abc")]
        public string Abc { get; set; }

        [Column("id")]
        [Key]
        public int Id { get; set; }

    }
}
