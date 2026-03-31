using Microsoft.EntityFrameworkCore;
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

        [Column("file_directory")]
        [Comment("Име на директория, в която се пращат файловете")]
        public string FileDirectory { get; set; }

        [Column("file_structure_type")]
        [Comment("Тип на структура на файла")]
        public int FileStructureType { get; set; }

        [Column("file_encoding_name")]
        [Comment("Encoding на файла")]
        public string FileEncodingName { get; set; }
    }
}
