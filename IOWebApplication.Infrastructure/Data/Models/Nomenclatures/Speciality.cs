using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Специалност на заседатели
    /// </summary>
    [Table("nom_speciality")]
    public class Speciality : BaseCommonNomenclature
    {
    [Column("lawunit_type_id")]
    public int? LawUnitTypeID { get; set; }

  }
}
