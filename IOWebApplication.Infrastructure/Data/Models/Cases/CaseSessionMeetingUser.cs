using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    /// <summary>
    /// Секретари към сесии на заседание
    /// </summary>
    [Table("case_session_meeting_user")]
    public class CaseSessionMeetingUser : UserDateWRT
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_session_meeting_id")]
        public int CaseSessionMeetingId { get; set; }

        [Column("secretary_user_id")]
        [Display(Name = "Секретар")]
        public string SecretaryUserId { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseSessionMeetingId))]
        public virtual CaseSessionMeeting CaseSessionMeeting { get; set; }

        [ForeignKey(nameof(SecretaryUserId))]
        public virtual ApplicationUser SecretaryUser { get; set; }
    }
}
