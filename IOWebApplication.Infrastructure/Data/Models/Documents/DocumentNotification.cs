// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace IOWebApplication.Infrastructure.Data.Models.Documents
{
    /// <summary>
    /// Съобщение/призовки/уведомяване към документ
    /// </summary>
    [Table("document_notification")]
    public class DocumentNotification : BaseInfo_DocumentNotification, IExpiredInfo
    {
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(DocumentResolutionId))]
        public virtual DocumentResolution DocumentResolution { get; set; }


        [ForeignKey(nameof(NotificationTypeId))]
        public virtual NotificationType NotificationType { get; set; }

        [ForeignKey(nameof(DocumentPersonId))]
        public virtual DocumentPerson DocumentPerson { get; set; }

        [ForeignKey(nameof(DocumentPersonLinkId))]
        public virtual DocumentPersonLink DocumentPersonLink { get; set; }

        [ForeignKey(nameof(NotificationAddressId))]
        public virtual Address NotificationAddress { get; set; }

        [ForeignKey(nameof(NotificationDeliveryGroupId))]
        public virtual NotificationDeliveryGroup NotificationDeliveryGroup { get; set; }

        [ForeignKey(nameof(NotificationStateId))]
        public virtual NotificationState NotificationState { get; set; }

        [ForeignKey(nameof(HtmlTemplateId))]
        public virtual HtmlTemplate HtmlTemplate { get; set; }

        public virtual ICollection<DeliveryItem> DeliveryItems { get; set; }

        public virtual ICollection<DocumentNotificationMLink> DocumentNotificationMLinks { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [NotMapped]
        [Display(Name = "Дата на връчване")]
        public DateTime? DeliveryDateVM { get { return DeliveryDate; } }

        [NotMapped]
        [Display(Name = "Данни за известяване")]
        public string DeliveryInfoVM { get { return DeliveryInfo; } }

        [NotMapped]
        [Display(Name = "Приложени документи")]
        public bool HaveAppendixVM { get { return HaveАppendix == true; } set { HaveАppendix = value; } }

        /// <summary>
        /// Данни за доставка при доставка от куриер/кметство 
        /// </summary>
        [NotMapped]
        [Display(Name = "Дата на последно посещение")]
        public DateTime? DeliveryDateCC { get; set; }

        [NotMapped]
        [Display(Name = "Данни за известяване")]
        public string DeliveryInfoCC { get; set; }
    }

    public class BaseInfo_DocumentNotification : UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("document_id")]
        public long? DocumentId { get; set; }

        [Column("document_resolution_id")]
        public long? DocumentResolutionId { get; set; }

        [Column("document_person_id")]
        [Display(Name = "Страни по дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете страна по дело")]
        public long? DocumentPersonId { get; set; }

        [Column("document_person_link_id")]
        [Display(Name = "Връзки по страни")]
        public int? DocumentPersonLinkId { get; set; }

        [Column("is_multi_link")]
        [Display(Name = "Множествена призовка")]
        public bool? IsMultiLink { get; set; }

        [Column("notification_address_id")]
        public long? NotificationAddressId { get; set; }

        [Column("document_person_address_id")]
        public long? DocumentPersonAddressId { get; set; }

        /// <summary>
        /// Вид съобщение, уведомление
        /// </summary>
        [Column("notification_type_id")]
        [Display(Name = "Вид известие")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете вид известие")]
        public int? NotificationTypeId { get; set; }

        /// <summary>
        /// Код за съд, година, номер за съда и баркод EAN13
        /// </summary>
        [Column("reg_number")]
        [Display(Name = "Регистрационен номер")]
        public string RegNumber { get; set; }
        /// <summary>
        /// Today
        /// </summary>
        [Column("reg_date")]
        [Display(Name = "Дата")]
        public DateTime RegDate { get; set; }

        /// <summary>
        /// Пореден номер на призовка/писмо за заседание
        /// </summary>
        [Display(Name = "Пореден номер на призовка/писмо")]
        [Column("notification_number")]
        public int? NotificationNumber { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        /// <summary>
        /// Вид доставка
        /// </summary>
        [Column("notification_delivery_group_id")]
        [Display(Name = "Вид известяване")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете вид известяване")]
        public int? NotificationDeliveryGroupId { get; set; }

  
        /// <summary>
        /// За връчване, връчена, невръчена
        /// </summary>
        [Column("notification_state_id")]
        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете статус")]
        public int NotificationStateId { get; set; }

        [Column("delivery_reason_id")]
        [Display(Name = "Причина")]
        public int? DeliveryReasonId { get; set; }

        [Column("date_send")]
        [Display(Name = "Дата на изпращане")]
        public DateTime? DateSend { get; set; }

        [Column("date_accepted")]
        [Display(Name = "Дата на приемане")]
        public DateTime? DateAccepted { get; set; }

        [Column("is_official_notification")]
        [Display(Name = "Официално уведомление")]
        public bool IsOfficialNotification { get; set; }

        /// <summary>
        /// Вид бланка
        /// </summary>
        [Column("html_template_id")]
        [Display(Name = "Бланка")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете бланка")]
        public int? HtmlTemplateId { get; set; }

        /// <summary>
        /// Данни за доставка
        /// </summary>
        [Column("delivery_date")]
        [Display(Name = "Дата на връчване")]
        public DateTime? DeliveryDate { get; set; }

        [Column("delivery_info")]
        [Display(Name = "Данни за известяване")]
        public string DeliveryInfo { get; set; }

        [Column("to_court_id")]
        [Display(Name = "Призовката ще се разнася от")]
        public int? ToCourtId { get; set; }

        [Column("lawunit_id")]
        [Display(Name = "Призовкар")]
        public int? LawUnitId { get; set; }

        [Column("delivery_area_id")]
        [Display(Name = "Район")]
        public int? DeliveryAreaId { get; set; }

        [Display(Name = "Посещение")]
        [Column("delivery_oper_id")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете посещение")]
        public int? DeliveryOperId { get; set; }

        /// <summary>
        ///  Данни за връщане Сканирано копие на върнатата призовка/ Дата и Допълнителна информация
        /// </summary>
        [Column("return_info")]
        [Display(Name = "Данни за връщане")]
        public string ReturnInfo { get; set; }

        [Column("return_document_id")]
        public long? ReturnDocumentId { get; set; }

        [Column("return_date")]
        [Display(Name = "Дата на връщане")]
        public DateTime? ReturnDate { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        [Column("date_print")]
        [Display(Name = "Дата на генериране пдф")]
        public DateTime? DatePrint { get; set; }

        [Column("have_appendix")]
        [Display(Name = "Приложени документи")]
        public bool? HaveАppendix { get; set; }

        [Column("is_from_email")]
        public bool? IsFromEmail { get; set; }

        [Column("notification_person_name")]
        public string NotificationPersonName { get; set; }
        [Column("notification_person_role")]
        public string NotificationPersonRole { get; set; }

        [Column("notification_link_name")]
        public string NotificationLinkName { get; set; }
  
    }
}
