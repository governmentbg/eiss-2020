using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Документи към движение: документи, актове, жалби
    /// </summary>
    [Table("case_migration_document")]
    public class CaseMigrationDocument : UserDateWRT, IExpiredInfo, IHaveId
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("case_migration_id")]
        public int CaseMigrationId { get; set; }
        /// <summary>
        /// Вид документ:SourceTypeSelectVM
        /// </summary>
        [Column("source_type")]
        public int SourceType { get; set; }

        /// <summary>
        /// Id на обект
        /// </summary>
        [Column("source_id")]
        public long SourceId { get; set; }

        /// <summary>
        /// Описание на прикачения обект: Молба 1/01.01.2026; Определение 78/14.02.2026;
        /// </summary>
        [Column("source_description")]
        public string SourceDescription { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CaseMigrationId))]
        public virtual CaseMigration CaseMigration { get; set; }

    }
}
