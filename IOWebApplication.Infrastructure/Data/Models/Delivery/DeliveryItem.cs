// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

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

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{

    /// <summary>
    /// Призовки/съобщения 
    /// </summary>
    [Table("delivery_item")]
    public class DeliveryItem : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("from_court_id")]
        [Display(Name = "Приета от")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете съд")]
        public int FromCourtId { get; set; }

        [Column("court_id")]
        [Display(Name = "За разнасяне от съд")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете съд за разнасяне")]
        public int CourtId { get; set; }

        [Column("date_send")]
        [Display(Name = "Дата на изпращане")]
        public DateTime? DateSend{ get; set; }

        [Column("date_accepted")]
        [Display(Name = "Дата на приемане")]
        public DateTime? DateAccepted { get; set; }

        [Column("delivery_date")]
        [Display(Name = "Дата на връчване")]
        public DateTime? DeliveryDate { get; set; }

        [Column("return_date")]
        [Display(Name = "Дата на връщане")]
        public DateTime? ReturnDate { get; set; }

        [Column("reg_number")]
        [Display(Name = "Регистрационен номер")]
        [Required(ErrorMessage = "Въведете регистрационен номер")]
        public string RegNumber { get; set; }

        [Column("reg_date")]
        [Display(Name = "Дата")]
        public DateTime? RegDate { get; set; }

        [Column("case_notification_id")]
        public int? CaseNotificationId { get; set; }

        [Column("delivery_area_id")]
        [Display(Name = "Район")]
        public int? DeliveryAreaId { get; set; }

        [Column("notification_state_id")]
        [Display(Name = "Статус")]
        public int NotificationStateId { get; set; }
        
        [Column("delivery_reason_id")]
        [Display(Name = "Причина")]
        public int? DeliveryReasonId { get; set; }

        [Column("lawunit_id")]
        [Display(Name = "Призовкар")]
        public int? LawUnitId { get; set; }

        [Column("person_name")]
        [Display(Name = "Име на лицето")]
        [Required(ErrorMessage = "Въведете име на лицето")]
        public string PersonName { get; set; }

        [Column("address_id")]
        public long AddressId { get; set; }
        
        [Display(Name = "Вид, номер и година на дело")]
        [Column("case_info")]
        public string CaseInfo { get; set; }
        
        [Column("html_template_id")]
        [Display(Name = "Бланка")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете бланка")]
        public int? HtmlTemplateId { get; set; }

        [Column("notification_type_id")]
        [Display(Name = "Вид известие")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете вид известие")]
        public int? NotificationTypeId { get; set; }

        [Display(Name = "Основен вид дело")]
        [Column("case_group_id")]
        public int? CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        [Column("case_type_id")]
        public int? CaseTypeId { get; set; }

        [Column("delivery_info")]
        [Display(Name = "Данни за уведомяване")]
        public string DeliveryInfo { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }
        
        /// <summary>
        /// Данни за доставка при доставка от куриер/кметство 
        /// </summary>
        [NotMapped]
        [Display(Name = "Дата на последно посещение")]
        public DateTime? DeliveryDateCC { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(FromCourtId))]
        public virtual Court FromCourt { get; set; }

        [ForeignKey(nameof(CaseNotificationId))]
        public virtual CaseNotification CaseNotification { get; set; }

        [ForeignKey(nameof(NotificationStateId))]
        public virtual NotificationState NotificationState { get; set; }

        [ForeignKey(nameof(AddressId))]
        public virtual Address Address { get; set; }

        [ForeignKey(nameof(DeliveryAreaId))]
        public virtual DeliveryArea DeliveryArea { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        public virtual ICollection<DeliveryItemOper> DeliveryItemOpers { get; set; }

        [ForeignKey(nameof(NotificationTypeId))]
        public virtual NotificationType NotificationType { get; set; }

        [ForeignKey(nameof(HtmlTemplateId))]
        public virtual HtmlTemplate HtmlTemplate { get; set; }

        [ForeignKey(nameof(CaseGroupId))]
        public virtual CaseGroup CaseGroup { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public virtual CaseType CaseType { get; set; }

   
        public DeliveryItem()
        {
            Address = new Address();
            Address.AddressTypeId = NomenclatureConstants.AddressType.Court;
        }
        public static bool IsNotificationStateForReturn(int notificationStateId)
        {
            int[] states = new int[]
            {
               NomenclatureConstants.NotificationState.Delivered,
               NomenclatureConstants.NotificationState.Delivered47,
               NomenclatureConstants.NotificationState.Delivered50,
               NomenclatureConstants.NotificationState.Delivered51,
               NomenclatureConstants.NotificationState.UnDelivered
            };
            return states.Contains(notificationStateId);
        }
        public static bool IsNotificationStateForOper(int notificationStateId)
        {
            if (IsNotificationStateForReturn(notificationStateId))
                return true;
            if (notificationStateId == NomenclatureConstants.NotificationState.ForDelivery)
                return true;
            if (notificationStateId == NomenclatureConstants.NotificationState.Visited)
                return true;
            return false;
        }
        public static bool IsNotificationStateForOperOnly(int notificationStateId)
        {
            if (IsNotificationStateForReturn(notificationStateId))
                return true;
            if (notificationStateId == NomenclatureConstants.NotificationState.Visited)
                return true;
            return false;
        }
    }
}
