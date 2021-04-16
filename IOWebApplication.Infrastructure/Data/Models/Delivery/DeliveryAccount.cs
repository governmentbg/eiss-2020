using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    /// <summary>
    /// Привързани мобилни устройства към потребител
    /// </summary>
    [Table("delivery_account")]
    public class DeliveryAccount : UserDateWRT
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("mobile_user_id")]
        public string MobileUserId { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("law_unit_id")]
        public int LawUnitId { get; set; }


        [Column("pin_hash")]
        public string PinHash { get; set; }

        [Column("mobile_token")]
        public string MobileToken { get; set; }

        [Column("api_address")]
        public string ApiAddress { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }
    }
}
