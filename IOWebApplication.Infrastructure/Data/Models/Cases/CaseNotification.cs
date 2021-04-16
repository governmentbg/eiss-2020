using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Data.Models.Identity;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Съобщение/призовки/уведомяване
    /// </summary>
    [Table("case_notification")]
    public class CaseNotification : BaseInfo_CaseNotification, IHaveHistory<CaseNotificationH>, IExpiredInfo
    {
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual CaseNotification Parent { get; set; }

        [ForeignKey(nameof(NotificationTypeId))]
        public virtual NotificationType NotificationType { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(CasePersonAddressId))]
        public virtual CasePersonAddress CasePersonAddress { get; set; }

        [ForeignKey(nameof(CasePersonLinkId))]
        public virtual CasePersonLink CasePersonLink { get; set; }

        [ForeignKey(nameof(CasePersonL1Id))]
        public virtual CasePerson CasePersonL1 { get; set; }

        [ForeignKey(nameof(CasePersonL2Id))]
        public virtual CasePerson CasePersonL2 { get; set; }

        [ForeignKey(nameof(CasePersonL3Id))]
        public virtual CasePerson CasePersonL3 { get; set; }

        [ForeignKey(nameof(CaseLawUnitId))]
        public virtual CaseLawUnit CaseLawUnit { get; set; }

        [ForeignKey(nameof(LawUnitAddressId))]
        public virtual Address LawUnitAddress { get; set; }

        [ForeignKey(nameof(NotificationAddressId))]
        public virtual Address NotificationAddress { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Document Document { get; set; }

        [ForeignKey(nameof(NotificationDeliveryGroupId))]
        public virtual NotificationDeliveryGroup GetNotificationDeliveryGroup { get; set; }

        [ForeignKey(nameof(NotificationDeliveryTypeId))]
        public virtual NotificationDeliveryType NotificationDeliveryType { get; set; }

        [ForeignKey(nameof(NotificationStateId))]
        public virtual NotificationState NotificationState { get; set; }

        [ForeignKey(nameof(HtmlTemplateId))]
        public virtual HtmlTemplate HtmlTemplate { get; set; }

        public ICollection<CaseNotificationH> History { get; set; }

        public virtual ICollection<DeliveryItem> DeliveryItems { get; set; }

        public virtual ICollection<CaseNotificationMLink> CaseNotificationMLinks { get; set; }
        
        public virtual ICollection<CaseNotificationComplain> CaseNotificationComplains { get; set; }

        [ForeignKey(nameof(LawUnitId))]
        public virtual LawUnit LawUnit { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }

        [ForeignKey(nameof(CaseSessionActComplainId))]
        public virtual CaseSessionActComplain CaseSessionActComplain { get; set; }

        [NotMapped]
        [Display(Name = "Дата на връчване")]
        public DateTime? DeliveryDateVM { get { return DeliveryDate; } }

        [NotMapped]
        [Display(Name = "Данни за известяване")]
        public string DeliveryInfoVM { get { return DeliveryInfo; } }

        [NotMapped]
        [Display(Name = "Приложени документи")]
        public bool HaveAppendixVM { get { return HaveАppendix == true; } set { HaveАppendix = value; } }
        
        [NotMapped]
        [Display(Name = "Печат на диспозитив")]
        public bool HaveDispositivVM { get { return HaveDispositiv == true; } set { HaveDispositiv = value; } }

        [NotMapped]
        [Display(Name = "Жалби към Акт/протокол")]
        public string MultiComplainIdVM { get; set; }

        [NotMapped]
         public string MultiComplainIdResultVM { get; set; }

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
    /// <summary>
    /// Съобщение/призовки/уведомяване - история
    /// </summary>
    [Table("case_notification_h")]
    public class CaseNotificationH : BaseInfo_CaseNotification, IHistory
    {
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("history_date_expire")]
        public DateTime? HistoryDateExpire { get; set; }

        [ForeignKey(nameof(Id))]
        public virtual CaseNotification CaseNotification { get; set; }
    }

    public class BaseInfo_CaseNotification : UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int? CaseSessionId { get; set; }

        [Column("case_session_act_id")]
        [Display(Name = "Акт/протокол")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Акт/протокол")]
        public int? CaseSessionActId { get; set; }

        /// <summary>
        /// Вид лице в съобщение: 1-към CasePerson;2-към CaseLawUnit
        /// </summary>
        [Column("notification_person_type")]
        public int NotificationPersonType { get; set; }

        [Column("case_person_id")]
        [Display(Name = "Страни по дело")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете страна по дело")]
        public int? CasePersonId { get; set; }

        [Column("case_person_link_id")]
        [Display(Name = "Връзки по страни")]
        public int? CasePersonLinkId { get; set; }

        /// <summary>
        /// Ред на представляване: CasePersonL1Id Чрез CasePersonL2Id  Чрез CasePersonL3Id
        /// </summary>

        [Column("case_person_l1_id")]
        [Display(Name = "Свързано лице")]
        public int? CasePersonL1Id { get; set; }


        [Column("case_person_l2_id")]
        [Display(Name = "Свързано лице")]
        public int? CasePersonL2Id { get; set; }

        [Column("case_person_l3_id")]
        [Display(Name = "Свързано лице")]
        public int? CasePersonL3Id { get; set; }

        [Column("is_multi_link")]
        [Display(Name = "Множествена призовка")]
        public bool? IsMultiLink { get; set; }
        
        [Column("link_direction_id")]
        [Display(Name = "Ред на представляване")]
        public int? LinkDirectionId { get; set; }

        [Column("link_direction_second_id")]
        [Display(Name = "Ред на представляване")]
        public int? LinkDirectionSecondId { get; set; }

        [Column("case_person_address_id")]
        [Display(Name = "Адрес на получаване")]
        //   [Range(1, int.MaxValue, ErrorMessage = "Изберете адрес на получаване")]
        public int? CasePersonAddressId { get; set; }

        [Column("case_lawunit_id")]
        [Display(Name = "Съдебен състав")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете лице от съдебен състав")]
        public int? CaseLawUnitId { get; set; }

        /// <summary>
        /// id към Address от адресите на заседателя
        /// </summary>
        [Column("lawunit_address_id")]
        [Display(Name = "Адрес на получаванe")]
        //  [Range(1, int.MaxValue, ErrorMessage = "Изберете адрес на получаване")]
        public long? LawUnitAddressId { get; set; }

        [Column("notification_person_name")]
        public string NotificationPersonName { get; set; }

        [Column("notification_person_duty")]
        public string NotificationPersonDuty { get; set; }

        [Column("notification_address_id")]
        public long? NotificationAddressId { get; set; }

        /// <summary>
        /// Свързан документ/призовка
        /// </summary>
        [Column("parent_id")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Вид призовка/съобщение, зависи от вид на дело, вид на заседание,статус
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
        /// Куриер
        /// </summary>
        [Column("notification_delivery_type_id")]
        [Display(Name = "Куриер")]
        public int? NotificationDeliveryTypeId { get; set; }

        [Column("courier_track_num")]
        [Display(Name = "Номер пратка при куриера")]
        public string CourierTrackNum { get; set; }

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
        /// Когато е писмо се извежда и документ
        /// </summary>
        [Column("document_id")]
        public long? DocumentId { get; set; }

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

        [Column("case_session_act_complain_id")]
        [Display(Name = "Жалба към Акт/протокол")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете жалба")]
        public int? CaseSessionActComplainId { get; set; }

        [Column("expert_report")]
        [Display(Name = "Описание на предмета и задачата на експеризата")]
        public string ExpertReport { get; set; }

        [Column("expert_dead_date")]
        [Display(Name = "Крайна дата за изговяне на експертиза")]
        public DateTime? ExpertDeadDate { get; set; }

        [Column("have_dispositiv")]
        [Display(Name = "Печат на диспозитив")]
        public bool? HaveDispositiv { get; set; }

        [Column("is_from_email")]
        public bool? IsFromEmail { get; set; }

        [Column("document_sender_person_id")]
        [Display(Name = "Подател нa съпровождащ документ")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете Подател нa съпровождащ документ")]
        public int? DocumentSenderPersonId { get; set; }

    }       
}
