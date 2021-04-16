using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Статус на разходен ордер
    /// </summary>
    [Table("nom_expense_order_state")]
    public class ExpenseOrderState: BaseCommonNomenclature
    {
    }
}
