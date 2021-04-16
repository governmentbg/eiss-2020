using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    /// <summary>
    /// Ид-та на задълженията, влезнали към един ордер
    /// </summary>
    [Table("money_expense_order_obligation")]
    public class ExpenseOrderObligation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("obligation_id")]
        public int ObligationId { get; set; }

        [Column("expense_order_id")]
        public int ExpenseOrderId { get; set; }

        [ForeignKey(nameof(ObligationId))]
        public virtual Obligation Obligation { get; set; }

        [ForeignKey(nameof(ExpenseOrderId))]
        public virtual ExpenseOrder ExpenseOrder { get; set; }

    }
}
