using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    [Table("case_notification_mlink")]
    public class CaseNotificationMLink
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("court_id")]
        public int? CourtId { get; set; }

        [Column("case_id")]
        public int? CaseId { get; set; }

        [Column("case_notification_id")]
        public int CaseNotificationId { get; set; }

        [Column("case_person_id")]
        public int? CasePersonId { get; set; }

        [Column("case_person_summoned_id")]
        public int? CasePersonSummonedId { get; set; }

        [Column("case_person_link_id")]
        public int? CasePersonLinkId { get; set; }

        [Column("person_name")]
        public string PersonSummonedName { get; set; }

        [Column("person_role")]
        public string PersonSummonedRole { get; set; }

        [Column("is_checked")]
        public bool IsChecked { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; }

        [NotMapped]
        [Display(Name = "Описане на връзката")]
        public string LinkLabel { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(CaseId))]
        public virtual Case Case { get; set; }

        [ForeignKey(nameof(CaseNotificationId))]
        public virtual CaseNotification CaseNotification { get; set; }

        [ForeignKey(nameof(CasePersonSummonedId))]
        public virtual CasePerson CasePersonSummoned { get; set; }

        [ForeignKey(nameof(CasePersonId))]
        public virtual CasePerson CasePerson { get; set; }
    }
}
