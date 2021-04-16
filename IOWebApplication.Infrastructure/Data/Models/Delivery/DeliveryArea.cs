using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Identity;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    /// <summary>
    /// Район за доставка
    /// </summary>
    [Table("delivery_area")]
    public class DeliveryArea : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("description")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Column("code")]
        [Display(Name = "Код")]
        public string Code { get; set; }
        
        [Display(Name = "Активен")]
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Display(Name = "Призовкар")]
        [Column("lawunit_id")]
        public int? LawUnitId { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [Column("date_from")]
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [NotMapped]
        public int CountAddress { get; set; }
    }
}
