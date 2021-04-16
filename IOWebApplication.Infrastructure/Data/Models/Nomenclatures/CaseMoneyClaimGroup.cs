﻿using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Основен вид на обстоятелство: Договор, Деликт, Неоснователно обогатяване, други
    /// </summary>
    [Table("nom_case_money_claim_group")]
    public class CaseMoneyClaimGroup : BaseCommonNomenclature
    {      

    }
}
