using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    [Table("case_session_notification_list")]
    public class CaseSessionNotificationList : UserDateWRT, IExpiredInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_session_id")]
        public int CaseSessionId { get; set; }

        [Column("notification_list_type_id")]
        public int? NotificationListTypeId { get; set; }

        /// <summary>
        /// 1-от страни по делото;2-от съдебен състав
        /// </summary>
        [Column("notification_person_type")]
        public int NotificationPersonType { get; set; }

        [Column("case_lawunit_id")]
        public int? CaseLawUnitId { get; set; }

        [Column("case_person_id")]
        public int? CasePersonId { get; set; }

        [Column("notification_address_id")]
        [Display(Name = "Адрес за призоваване")]
        public long? NotificationAddressId { get; set; }

        [Column("row_number")]
        public int RowNumber { get; set; }

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

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(CaseLawUnitId))]
        public virtual CaseLawUnit CaseLawUnit { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(NotificationAddressId))]
        public virtual Address NotificationAddress { get; set; }
    }
}
