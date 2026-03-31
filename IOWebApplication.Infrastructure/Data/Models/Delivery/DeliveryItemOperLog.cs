using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Constants;
using System.Linq;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Documents;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    /// <summary>
    /// Деиствия/операции по Призовки/съобщения 
    /// </summary>
    [Table("delivery_item_oper_log")]
    public class DeliveryItemOperLog : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("reg_number")]
        public string RegNumber { get; set; }

        [Column("reg_date")]
        public DateTime? RegDate { get; set; }

        [Column("case_info")]
        public string CaseInfo { get; set; }

        [Column("delivery_item_id")]
        public int? DeliveryItemId { get; set; }

        [Column("delivery_item_oper_id")]
        public int? DeliveryItemOperId { get; set; }

        [Column("case_notification_id")]
        public int? CaseNotificationId { get; set; }

        [Column("document_notification_id")]
        public int? DocumentNotificationId { get; set; }

        [Column("court_wrt_id")]
        public int CourtWrtId { get; set; }

        [Column("from_court_id")]
        public int? FromCourtId { get; set; }

        [Column("to_court_id")]
        public int? ToCourtId { get; set; }

        [Column("delivery_oper_id")]
        public int? DeliveryOperId { get; set; }

        [Column("delivery_area_id")]
        public int? DeliveryAreaId { get; set; }

        [Column("notification_state_id")]
        public int NotificationStateId { get; set; }

        [Column("long")]
        public string Long { get; set; }

        [Column("lat")]
        public string Lat { get; set; }

        [Column("lawunit_id")]
        public int? LawUnitId { get; set; }

        [Column("delivery_info")]
        [Display(Name = "Данни за уведомяване")]
        public string DeliveryInfo { get; set; }

        [Column("delivery_reason_id")]
        [Display(Name = "Причина")]
        public int? DeliveryReasonId { get; set; }

        [Column("date_oper")]
        [Display(Name = "Дата на операция")]
        public DateTime? DateOper { get; set; }
        
        [Column("screen_label")]
        [Display(Name = "Въведено е екран")]
        public string PageLabel { get; set; }
        [Column("screen_url")]
        public string PageUrl { get; set; }

        [Column("action")]
        [Display(Name = "Действие")]
        public string Action { get; set; }
        [Column("is_from_mobile")]
        public bool? IsFromMobile { get; set; }

        [Display(Name = "Основен вид дело")]
        [Column("case_group_id")]
        public int? CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        [Column("case_type_id")]
        public int? CaseTypeId { get; set; }

        [Display(Name = "Призован")]
        [Column("person_name")]
        public string PersonName { get; set; }

        [Display(Name = "Адрес")]
        [Column("address_str")]
        public string AddressStr { get; set; }

        public int? NotificationDeliveryGroupId { get; set; }

        [ForeignKey(nameof(CourtWrtId))]
        public virtual Court CourtWrt { get; set; }

        [ForeignKey(nameof(NotificationStateId))]
        public virtual NotificationState NotificationState { get; set; }

        [ForeignKey(nameof(DeliveryOperId))]
        public virtual DeliveryOper DeliveryOper { get; set; }

        [ForeignKey(nameof(DeliveryAreaId))]
        public virtual DeliveryArea DeliveryArea { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(DeliveryItemId))]
        public virtual DeliveryItem DeliveryItem { get; set; }

        [ForeignKey(nameof(DeliveryReasonId))]
        public virtual DeliveryReason DeliveryReason { get; set; }

        [ForeignKey(nameof(DeliveryItemOperId))]
        public virtual DeliveryItemOper DeliveryItemOper { get; set; }
    }
}
