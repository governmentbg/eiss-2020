using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// 1 - Искане за достъп до дело, 2 - Искане за получаване на електронни призовки и съобщение
    /// </summary>
    [Table("nom_decision_request_type")]
    public class DecisionRequestType : BaseCommonNomenclature
    {
    }
}
