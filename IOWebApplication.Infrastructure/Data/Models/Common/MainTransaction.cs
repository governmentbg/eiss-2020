using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_main_transaction")]
    public class MainTransaction : UserDateWRT
    {
        public MainTransaction()
        {

        }

        public MainTransaction(long mainGroupId, int operationTypeId)
        {
            MainGroupId = mainGroupId;
            PriorOperationTypeId = operationTypeId;
            OperationTypeId = operationTypeId;
            DateWrt = DateTime.Now;
        }


        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("main_group_id")]
        public long MainGroupId { get; set; }

        [Column("prior_operation_type_id")]
        public int PriorOperationTypeId { get; set; }

        [Column("operation_type_id")]
        public int OperationTypeId { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [ForeignKey(nameof(MainGroupId))]
        public virtual MainGroup MainGroup { get; set; }

        public virtual MainGroup MainGroupLastTransaction { get; set; }

        [ForeignKey(nameof(PriorOperationTypeId))]
        public virtual TransactionOperationType PriorOperation { get; set; }

        [ForeignKey(nameof(OperationTypeId))]
        public virtual TransactionOperationType Operation { get; set; }

    }
}
