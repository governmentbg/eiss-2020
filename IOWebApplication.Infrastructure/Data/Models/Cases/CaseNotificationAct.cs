using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Cases
{
    [Table("case_notification_act")]
    public class CaseNotificationAct : UserDateWRT
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("case_notification_id")]
        public int CaseNotificationId { get; set; }


        [Column("is_checked")]
        public bool IsChecked { get; set; }

        [Column("case_session_act_id")]
        public int CaseSessionActId { get; set; }

        [ForeignKey(nameof(CaseNotificationId))]
        public virtual CaseNotification CaseNotification { get; set; }

        [ForeignKey(nameof(CaseSessionActId))]
        public virtual CaseSessionAct CaseSessionAct { get; set; }
    }
}
