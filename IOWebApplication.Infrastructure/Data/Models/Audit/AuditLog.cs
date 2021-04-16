using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Audit
{
    [Table("audit_log", Schema = "audit_log")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("inserted_date")]
        public DateTime InsertedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [Column("data", TypeName = "jsonb")]
        public string Data { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("method")]
        public string Method { get; set; }

        [Column("request_url")]
        public string RequestUrl { get; set; }

        [Column("client_ip")]
        public string ClientIP { get; set; }

        [Column("user_ip")]
        public string UserId { get; set; }

        [Column("full_name")]
        public string FullName { get; set; }

        [Column("operation")]
        public string Operation { get; set; }

        [Column("object_type")]
        public string ObjectType { get; set; }

        [Column("object_info")]
        public string ObjectInfo { get; set; }

        [Column("base_object")]
        public string BaseObject { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public AuditLog()
        {
            Data = "{}";
        }
    }
}
