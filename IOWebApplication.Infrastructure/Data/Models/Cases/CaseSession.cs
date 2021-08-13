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
    /// Заседания по делото
    /// </summary>
    [Table("case_session")]
    public class CaseSession : BaseInfo_CaseSession, IHaveHistory<CaseSessionH>
    {
        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }


        [ForeignKey(nameof(SessionTypeId))]
        public virtual SessionType SessionType { get; set; }

        [ForeignKey(nameof(CourtHallId))]
        public virtual CourtHall CourtHall { get; set; }

        [ForeignKey(nameof(SessionStateId))]
        public virtual SessionState SessionState { get; set; }

        public virtual ICollection<CasePerson> CasePersons { get; set; }
        public virtual ICollection<CaseSessionResult> CaseSessionResults { get; set; }
        public virtual ICollection<CaseSessionH> History { get; set; }
        public virtual ICollection<CaseSessionAct> CaseSessionActs { get; set; }

        [InverseProperty(nameof(CaseLawUnit.CaseSession))]
        public virtual ICollection<CaseLawUnit> CaseLawUnits { get; set; }

        [InverseProperty(nameof(CaseLawUnit.FromCaseSession))]
        public virtual ICollection<CaseLawUnit> FromCaseLawUnits { get; set; }
        [InverseProperty(nameof(CaseLawUnit.ToCaseSession))]
        public virtual ICollection<CaseLawUnit> ToCaseLawUnits { get; set; }
        public virtual ICollection<CaseSessionDoc> CaseSessionDocs { get; set; }
        public virtual ICollection<CaseSessionMeeting> CaseSessionMeetings { get; set; }

        public CaseSession()
        {
            CasePersons = new HashSet<CasePerson>();
            CaseSessionResults = new HashSet<CaseSessionResult>();
            CaseSessionActs = new HashSet<CaseSessionAct>();
            CaseLawUnits = new HashSet<CaseLawUnit>();
            CaseSessionMeetings = new HashSet<CaseSessionMeeting>();
            CaseSessionDocs = new HashSet<CaseSessionDoc>();
        }
    }

    /// <summary>
    /// Заседания по делото - история
    /// </summary>
    [Table("case_session_h")]
    public class CaseSessionH : BaseInfo_CaseSession, IHistory
    {
        [Column("history_id")]
        public int HistoryId { get; set; }

        [Column("history_date_expire")]
        public DateTime? HistoryDateExpire { get; set; }

        [ForeignKey(nameof(Id))]
        public virtual CaseSession CaseSession { get; set; }
    }

    public class BaseInfo_CaseSession : UserDateWRT, IExpiredInfo
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        /// <summary>
        /// Вид заседания: Закрито,Първо,второ,друго
        /// </summary>
        [Column("session_type_id")]
        [Display(Name = "Вид заседаниe")]
        public int SessionTypeId { get; set; }


        [Column("court_hall_id")]
        [Display(Name = "Зала")]
        public int? CourtHallId { get; set; }

        /// <summary>
        /// Дата и час
        /// </summary>
        [Column("date_from")]
        [Display(Name = "Начало")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "Край")]
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Статус на заседание: насрочено, отложено
        /// </summary>
        [Column("session_state_id")]
        [Display(Name = "Статус на заседание")]
        public int SessionStateId { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Column("date_expired")]
        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Column("user_expired_id")]
        public string UserExpiredId { get; set; }

        [Column("description_expired")]
        [Display(Name = "Причина за анулиране")]
        public string DescriptionExpired { get; set; }

        /// <summary>
        /// Само за НД, ВКС при избор 1 - Промяна на състава, се изтегля нов състав по делото според графика
        /// 1 - промяна
        /// null,2 - без промяна
        /// </summary>
        [Display(Name = "Промяна на състава по делото")]
        [Column("case_lawunit_change")]
        public int? CaseLawunitChange { get; set; }

        [ForeignKey(nameof(UserExpiredId))]
        public virtual ApplicationUser UserExpired { get; set; }
    }
}
