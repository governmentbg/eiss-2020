// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид претендиран разноски: Държавна такса, адвокатско възнаграждение, юрисконсултско възнаграждение, други
    /// </summary>
    [Table("nom_case_money_expense_type")]
    public class CaseMoneyExpenseType : BaseCommonNomenclature
    {
        
    }
}
