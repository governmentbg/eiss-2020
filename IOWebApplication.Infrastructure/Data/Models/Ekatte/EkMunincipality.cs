// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models
{
    [Table("ek_munincipalities")]
    public class EkMunincipality
    {
        [Column("municipality_id")]
        [Key]
        public int MunicipalityId { get; set; }

        [Column("municipality")]
        public string Municipality { get; set; }

        [Column("ekatte")]
        public string Ekatte { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("document")]
        public string Document { get; set; }

        [Column("abc")]
        public string Abc { get; set; }

        [Column("district_id")]
        public int? DistrictId { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public EkDistrict District { get; set; }
    }
}
