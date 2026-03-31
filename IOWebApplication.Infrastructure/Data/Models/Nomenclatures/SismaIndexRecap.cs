// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    [Table("nom_sisma_index_recap")]
    public class SismaIndexRecap
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("sheet_index")]
        public int SheetIndex { get; set; }

        [Column("sisma_index")]
        public string SismaIndex { get; set; }

        [Column("order_number")]
        public int OrderNumber { get; set; }

        public virtual ICollection<SismaIndexRecapContains> RecapContains { get; set; }

        public SismaIndexRecap()
        {
            RecapContains = new HashSet<SismaIndexRecapContains>();
        }
    }
}
