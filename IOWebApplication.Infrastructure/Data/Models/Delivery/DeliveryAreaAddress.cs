using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using Microsoft.AspNetCore.Mvc;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Identity;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    /// <summary>
    ///Адреси в район за призовки
    /// </summary>
    [Table("delivery_area_address")]
    public class DeliveryAreaAddress : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("delivery_area_id")]
        public int DeliveryAreaId { get; set; }

        [Column("city_code")]
        [Display(Name = "Населено място")]
        public string CityCode { get; set; }

        [Column("residential_area_code")]
        [Display(Name = "Квартал/ж.к.")]
        public string ResidentionAreaCode { get; set; }

        [Column("street_code")]
        [Display(Name = "Улица")]
        public string StreetCode { get; set; }

        [Display(Name = "Тип номера")]
        [Column("number_type")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете тип номера")]
        public int? NumberType { get; set; }

        [Display(Name = "От номер")]
        [Column("number_from")]
        [Remote(action: "VerifyNumberFrom", controller: "DeliveryAreaAddress", AdditionalFields = nameof(NumberType)+","+ nameof(NumberTo))]
        public int? NumberFrom { get; set; }

        [Display(Name = "До номер")]
        [Column("number_to")]
        [Remote(action: "VerifyNumberTo", controller: "DeliveryAreaAddress", AdditionalFields = nameof(NumberType) + "," + nameof(NumberFrom))]
        public int? NumberTo { get; set; }
        
        [Column("block_name")]
        [Display(Name = "Име на блок")]
        public string BlockName { get; set; }

        [Display(Name = "Активен")]
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

        [Display(Name = "От дата")]
        [Column("date_from")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        [Column("date_to")]
        public DateTime? DateTo { get; set; }

        [ForeignKey(nameof(DeliveryAreaId))]
        public virtual DeliveryArea DeliveryArea { get; set; }
        
        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
    }
}
