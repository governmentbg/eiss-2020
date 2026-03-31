using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Видове действия на опашка от операции
    /// </summary>
    [Table("nom_transaction_operation_type")]
    public class TransactionOperationType : BaseCommonNomenclature
    {
    }
}
