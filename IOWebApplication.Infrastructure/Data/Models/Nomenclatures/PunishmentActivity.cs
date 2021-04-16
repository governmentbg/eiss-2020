﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Активност на наказанието: замененено,кумулирано, наложено,отменено и посагено
    /// </summary>
    [Table("nom_punishement_activity")]
    public class PunishmentActivity : BaseCommonNomenclature
    {
       
    }
}
