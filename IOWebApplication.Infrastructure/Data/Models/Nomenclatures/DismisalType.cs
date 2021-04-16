using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове отвод на съдия/заседател
    /// </summary>
    [Table("nom_dismisal_type")]
    public class DismisalType : BaseCommonNomenclature
    {
    [Column("dismisal_kind_id")]
    public int? DismisalKindId { get; set; }

   
  }
}
