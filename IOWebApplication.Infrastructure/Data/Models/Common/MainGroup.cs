using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_main_group")]
    public class MainGroup : UserDateWRT
    {
        public MainGroup()
        {
            Transactions = new HashSet<MainTransaction>();
        }
        public MainGroup(int courtId, int sourceType, long sourceId, long parentSourceId = 0)
        {
            CourtId = courtId;
            SourceType = sourceType;
            SourceId = sourceId;
            DateWrt = DateTime.Now;
            ParentSourceId = parentSourceId;
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("source_type")]
        public int SourceType { get; set; }

        [Column("source_id")]
        public long SourceId { get; set; }

        [Column("parent_source_id")]
        public long? ParentSourceId { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("last_transaction_id")]
        public long? LastTransationId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(LastTransationId))]
        public virtual MainTransaction LastTransation { get; set; }

        public virtual ICollection<MainTransaction> Transactions { get; set; }
    }
}
