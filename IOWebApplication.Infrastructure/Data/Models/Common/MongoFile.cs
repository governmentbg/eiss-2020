using IOWebApplication.Infrastructure.Data.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    [Table("common_mongo_file")]
    public class MongoFile : IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("file_id")]
        public string FileId { get; set; }
        [Column("source_type")]
        public int SourceType { get; set; }
        [Column("source_id")]
        public string SourceId { get; set; }
        [Column("source_id_number")]
        public long SourceIdNumber { get; set; }
        [Column("file_name")]
        public string FileName { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("file_size")]
        public int FileSize { get; set; }
        [Column("signers_count")]
        public int? SignersCount { get; set; }
        [Column("signitures_count")]
        public int? SignituresCount { get; set; }
        [Column("date_uploaded")]
        public DateTime DateUploaded { get; set; }

        [Column("user_uploaded")]
        public string UserUploaded { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("mongo_file_type_id")]
        public int? MongoFileTypeId { get; set; }

        ///// <summary>
        ///// Състояние на антивирусна проверка
        ///// null-не подлежи на проверка;1-проверява се;2-проверен;3-заразен
        ///// </summary>
        //[Column("av_state")]
        //public byte? AVState { get; set; } = null;

        ///// <summary>
        ///// Код за статус на антивирусна проверка
        ///// </summary>
        //[MaxLength(50)]
        //[Column("av_key")]
        //public string av_key { get; set; } = null;

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(MongoFileTypeId))]
        public virtual MongoFileType MongoFileType { get; set; }
    }
}
