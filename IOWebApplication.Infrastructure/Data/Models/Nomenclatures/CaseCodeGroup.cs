using IOWebApplication.Infrastructure.Data.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Групи от шифри по точен вид дело
    /// </summary>
    [Table("nom_case_code_group")]
    public class CaseCodeGroup : UserDateWRT
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("group_alias")]
        public string GroupAlias { get; set; }

        [Column("case_code_id")]
        public int CaseCodeId { get; set; }

        [ForeignKey(nameof(CaseCodeId))]
        [Display(Name = "Шифър")]
        public virtual CaseCode CaseCode { get; set; }
    }
}
