using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Money
{
    /// <summary>
    /// ИЛ-ве към Приемо предавателен протокол
    /// </summary>
    [Table("money_exchange_doc_exec_list")]
    public class ExchangeDocExecList
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("exec_list_id")]
        public int ExecListId { get; set; }

        [Column("exchange_doc_id")]
        public int ExchangeDocId { get; set; }

        [ForeignKey(nameof(ExecListId))]
        public virtual ExecList ExecList { get; set; }

        [ForeignKey(nameof(ExchangeDocId))]
        public virtual ExchangeDoc ExchangeDoc { get; set; }
    }
}
