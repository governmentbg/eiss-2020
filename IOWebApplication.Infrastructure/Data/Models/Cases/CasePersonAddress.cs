using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Адреси към страна по делото
    /// </summary>
    [Table("case_person_address")]
    public class CasePersonAddress : BaseInfo_CasePersonAddress, IHaveHistory<CasePersonAddressH>, IExpiredInfo
    {
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }

        [ForeignKey(nameof(AddressId))]
        public virtual Address Address { get; set; }
        public virtual ICollection<CasePersonAddressH> History { get; set; }

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
    }
    /// <summary>
    /// Адреси към страна по делото - история
    /// </summary>
    [Table("case_person_address_h")]
    public class CasePersonAddressH : BaseInfo_CasePersonAddress, IHistory
    {
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("history_date")]
        public DateTime HistoryDate { get; set; }

        [Column("history_date_expire")]
        public DateTime? HistoryDateExpire { get; set; }

        [ForeignKey(nameof(Id))]
        public virtual CasePersonAddress CasePersonAddress { get; set; }
    }

    public class BaseInfo_CasePersonAddress : UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_person_id")]
        public int CasePersonId { get; set; }

        [Column("address_id")]
        public long AddressId { get; set; }

        [Column("for_notification")]
        [Display(Name = "Адрес за известие")]
        public bool? ForNotification { get; set; }

        /// <summary>
        /// Guid на адрес, при добавяне в делото се генерира и се пренася във всички заседания
        /// </summary>
        [Column("case_person_address_identificator")]
        public string CasePersonAddressIdentificator { get; set; }

    }
}
