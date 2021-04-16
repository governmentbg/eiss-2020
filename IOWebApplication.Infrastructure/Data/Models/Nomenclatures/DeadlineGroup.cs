﻿using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основен вид срок по дела: 1-за съдии,2-за лица
    /// </summary>
    [Table("nom_deadline_group")]
    public class DeadlineGroup : BaseCommonNomenclature
    {
     
    }
}
