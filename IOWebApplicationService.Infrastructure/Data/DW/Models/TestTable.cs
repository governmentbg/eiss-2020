using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Data.DW.Models
{
    [Table("_test_table")]
    public class TestTable
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("label")]
        public string Label { get; set; }
    }
}
