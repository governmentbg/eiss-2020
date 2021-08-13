// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_court_deliverer")]
    public class CourtDeliverer
    {
        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("ekatte")]
        [Required]
        public string Ekatte { get; set; }

        [Column("deiverer_court_id")]
        public int DeivererCourtId { get; set; }
    }
}
