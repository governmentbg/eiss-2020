using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{

    /// <summary>
    /// Причина за недоставени призовки/съобщения 
    /// </summary>
    [Table("nom_delivery_reason")]
    public class DeliveryReason : BaseCommonNomenclature
    {
    }
}
