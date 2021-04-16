using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Възраст на лицето: Пълнолетен, непълнолетен, малолетен
    /// </summary>
    [Table("nom_person_maturity")]
    public class PersonMaturity : BaseCommonNomenclature
    {
       
    }
}
