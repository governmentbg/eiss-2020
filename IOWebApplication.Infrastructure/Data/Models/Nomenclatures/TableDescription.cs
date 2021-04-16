using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Описание на базата данни
    /// </summary>
    [Table("nom_table_description")]
    public class TableDescription
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("table_name")]
        public string TableName { get; set; }

        [Column("ordinal_position")]
        public int OrdinalPosition { get; set; }

        [Column("column_name")]
        public string ColumnName { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("data_type")]
        public string DataType { get; set; }

        [Column("data_type_normalized")]
        public string DataTypeNormalized { get; set; }

        [Column("is_nullable")]
        public bool IsNullable { get; set; }
    }
}
