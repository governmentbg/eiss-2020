using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид на фирма
    /// </summary>
    [Table("nom_company_type")]
    public class CompanyType : BaseCommonNomenclature
    {
    }
}
