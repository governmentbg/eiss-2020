// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{

    [Table("nom_sisma_index_recap_contains")]
    public class SismaIndexRecapContains
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("sisma_index_recap_id")]
        public int SismaIndexRecapId { get; set; }

        [Column("sisma_index_contains")]
        public string SismaIndexContains { get; set; }

        [Column("sign")]
        public int Sign { get; set; }

        [ForeignKey(nameof(SismaIndexRecapId))]
        public virtual SismaIndexRecap SismaIndexRecap { get; set; }
    }
}
