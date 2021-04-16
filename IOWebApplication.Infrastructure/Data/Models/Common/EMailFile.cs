using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_email_file")]
    public class EMailFile 
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("email_message_id")]
        public int EMailMessageId { get; set; }
        
        [Column("mongo_file_id")]
        public int MongoFileId { get; set; }

        [ForeignKey(nameof(EMailMessageId))]
        public virtual EMailMessage EMailMessage { get; set; }

        [ForeignKey(nameof(MongoFileId))]
        public virtual MongoFile MongoFile { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }
    }
}
