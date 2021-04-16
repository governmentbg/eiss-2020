using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на документ в заседание: Неразгледан,разгледан,окончателно разгледан
    /// </summary>
    [Table("nom_session_doc_state")]
    public class SessionDocState : BaseCommonNomenclature
    {
       
    }
}
