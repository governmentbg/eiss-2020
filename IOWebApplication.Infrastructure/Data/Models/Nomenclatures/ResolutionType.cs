using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Номенклатура Разпореждания по документи
    /// </summary>
    [Table("nom_resolution_type")]
    public class ResolutionType : BaseCommonNomenclature
    {
    }
}
