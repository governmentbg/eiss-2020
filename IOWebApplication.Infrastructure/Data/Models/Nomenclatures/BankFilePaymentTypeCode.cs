using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Тип на операцията - Кредит/Дебит
    /// </summary>
    [Table("nom_bank_file_payment_type_code")]
    public class BankFilePaymentTypeCode : BaseCommonNomenclature
    {
    }
}
