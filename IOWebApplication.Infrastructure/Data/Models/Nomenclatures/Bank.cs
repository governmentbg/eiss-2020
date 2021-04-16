using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Списък с банки
    /// </summary>
    [Table("nom_bank")]
    public class Bank
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("label")]
        public string Label { get; set; }

        [Column("bic")]
        public string BIC { get; set; }

        [Column("code_for_search")]
        public string CodeForSearch { get; set; }
    }
}
