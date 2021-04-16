using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Вид суми по дело
    /// </summary>
    [Table("nom_money_type")]
    public class MoneyType : BaseCommonNomenclature
    {
        /// <summary>
        /// 1:Приход,-1:Разход
        /// </summary>
        [Column("money_sign")]
        public int MoneySign { get; set; }
        
        [Column("money_group_id")]
        public int MoneyGroupId { get; set; }

        [Column("no_money")]
        public bool? NoMoney { get; set; }

        [Column("is_earning")]
        public bool? IsEarning { get; set; }

        [Column("is_transport")]
        public bool? IsTransport { get; set; }

        [ForeignKey(nameof(MoneyGroupId))]
        public virtual MoneyGroup MoneyGroup { get; set; }
    }
}
