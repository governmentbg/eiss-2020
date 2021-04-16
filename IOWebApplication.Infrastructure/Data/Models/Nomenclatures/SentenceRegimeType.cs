using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Режим на изтърпяване на присъда: общ,лек и строг
    /// </summary>
    [Table("nom_sentence_regime_type")]
    public class SentenceRegimeType : BaseCommonNomenclature
    {      

    }
}
