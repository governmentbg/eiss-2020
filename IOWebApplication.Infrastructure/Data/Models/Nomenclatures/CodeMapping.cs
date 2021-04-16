using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Стойности на външни номенклатури
    /// </summary>
    [Table("nom_code_mapping")]
    public class CodeMapping
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Display(Name = "Код на съответствие")]
        [Column("alias")]
        public string Alias { get; set; }

        [Display(Name = "Външен код")]
        [Column("outer_code")]
        public string OuterCode { get; set; }

        [Display(Name = "Вътрешна стойност")]
        [Column("inner_code")]
        public string InnerCode { get; set; }

        [Display(Name = "Описание")]
        [Column("description")]
        public string Description { get; set; }
    }
}
