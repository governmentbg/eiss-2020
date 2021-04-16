using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Тайни съвещания/сесии към заседание
    /// </summary>
    [Table("case_session_meeting")]
    public class CaseSessionMeeting : UserDateWRT, IExpiredInfo
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

        [Column("session_meeting_type_id")]
        [Display(Name = "Тип сесия")]
        public int SessionMeetingTypeId { get; set; }

        [Column("date_from")]
        [Display(Name = "От дата")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateFrom { get; set; }

        [Column("date_to")]
        [Display(Name = "До дата")]
        [Required(ErrorMessage = "Въведете {0}.")]
        public DateTime DateTo { get; set; }

        [Column("description")]
        [Display(Name = "Забележка")]
        public string Description { get; set; }

        [Column("is_auto_create")]
        public bool? IsAutoCreate { get; set; }

        [Column("court_hall_id")]
        [Display(Name = "Зала")]
        public int? CourtHallId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionId))]
        public virtual CaseSession CaseSession { get; set; }

        [ForeignKey(nameof(SessionMeetingTypeId))]
        public virtual SessionMeetingType SessionMeetingType { get; set; }

        [ForeignKey(nameof(CourtHallId))]
        public virtual CourtHall CourtHall { get; set; }

        public virtual ICollection<CaseSessionMeetingUser> CaseSessionMeetingUsers { get; set; }

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

        public CaseSessionMeeting()
        {
            CaseSessionMeetingUsers = new HashSet<CaseSessionMeetingUser>();
        }

    }
}
