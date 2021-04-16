using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Характер на делото, за номерация
    /// </summary>
    [Table("nom_case_character")]
    public class CaseCharacter : BaseCommonNomenclature
    {
        //[Column("case_instance_id")]
        //[Display(Name = "Инстанция")]
        //public int CaseInstanceId { get; set; }

        //[ForeignKey(nameof(CaseInstanceId))]
        //public virtual CaseInstance CaseInstance { get; set; }

    }
}
