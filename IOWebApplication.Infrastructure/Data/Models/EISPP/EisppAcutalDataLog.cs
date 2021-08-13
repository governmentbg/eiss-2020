// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.EISPP
{
    [Table("common_eispp_actual_data_log")]
    public class EisppAcutalDataLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("eispp_number")]
        public string EISSPNumber { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [Column("error_description")]
        public string ErrorDescription { get; set; }

        [Column("response")]
        public string Response { get; set; }
    }
}
