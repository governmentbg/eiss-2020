using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Страни по делото
    /// </summary>
    [Table("case_person")]
    public class CasePerson : BaseInfo_CasePerson, IHaveHistory<CasePersonH>, IExpiredInfo
    {
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSelectionProtokolId))]
        public virtual CaseSelectionProtokol CaseSelectionProtokol { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(PersonRoleId))]
        public virtual PersonRole PersonRole { get; set; }

        [ForeignKey(nameof(MilitaryRangId))]
        public virtual MilitaryRang MilitaryRang { get; set; }

        [ForeignKey(nameof(PersonMaturityId))]
        public virtual PersonMaturity PersonMaturity { get; set; }

        [ForeignKey(nameof(CompanyTypeId))]
        public virtual CompanyType CompanyType { get; set; }

        public virtual ICollection<CasePersonAddress> Addresses { get; set; }
        public virtual ICollection<CasePersonH> History { get; set; }
        public virtual ICollection<CasePersonSentence> CasePersonSentences { get; set; }
        public virtual ICollection<CasePersonCrime> CasePersonCrimes { get; set; }


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

        public CasePerson()
        {
            Addresses = new HashSet<CasePersonAddress>();
            CasePersonCrimes = new HashSet<CasePersonCrime>();
        }
    }
    /// <summary>
    /// Страни по делото - история
    /// </summary>
    [Table("case_person_h")]
    public class CasePersonH : BaseInfo_CasePerson, IHistory
    {
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("history_date_expire")]
        public DateTime? HistoryDateExpire { get; set; }

        [ForeignKey(nameof(Id))]
        public virtual CasePerson CasePerson { get; set; }
    }
    public class BaseInfo_CasePerson : PersonNamesBase, IUserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("case_session_id")]
        public int? CaseSessionId { get; set; }

        [Column("case_selection_protokol_id")]
        public int? CaseSelectionProtokolId { get; set; }

        [Column("person_role_id")]
        public int PersonRoleId { get; set; }

        [Column("military_rang_id")]
        public int? MilitaryRangId { get; set; }

        [Column("person_maturity_id")]
        public int? PersonMaturityId { get; set; }

        /// <summary>
        /// Първоначална страна по делото, добавена със създаването му
        /// </summary>
        [Column("is_initial_person")]
        public bool IsInitialPerson { get; set; }

        /// <summary>
        /// Guid на лице, при добавяне в делото се генерира и се пренася във всички заседания
        /// </summary>
        [Column("case_person_identificator")]
        public string CasePersonIdentificator { get; set; }

        [Column("date_from")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        public DateTime? DateTo { get; set; }

        [Column("row_number")]
        public int RowNumber { get; set; }

        /// <summary>
        /// Да бъде призован
        /// </summary>
        [Column("for_notification")]
        public bool? ForNotification { get; set; }

        [Column("notification_number")]
        public int? NotificationNumber { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }
        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }

        [Column("date_transfered_dw")]
        public DateTime? DateTransferedDW { get; set; }

        /// <summary>
        /// Задържан
        /// </summary>
        [Column("is_arrested")]
        public bool? IsArrested { get; set; }

        //Място на раждане
        [Column("birth_country_code")]
        [Display(Name = "Държава")]
        public string BirthCountryCode { get; set; }

        [Column("birth_city_code")]
        [Display(Name = "Населено място")]
        public string BirthCityCode { get; set; }

        //Ако не е България - текстово поле
        [Column("birth_foreign_place")]
        [Display(Name = "Населено място")]
        public string BirthForeignPlace { get; set; }

        [Column("company_type_id")]
        public int? CompanyTypeId { get; set; }

        [Column("tax_number")]
        [Display(Name = "Данъчен номер")]
        public string TaxNumber { get; set; }

        [Column("re_register_date")]
        [Display(Name = "Дата на пререгистрация в АВ")]
        public DateTime? ReRegisterDate { get; set; }

        [Column("is_deceased")]
        [Display(Name = "Починало лице")]
        public bool? IsDeceased { get; set; }
    }
}
