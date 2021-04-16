using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseNotificationVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int? CaseSessionId { get; set; }
        public int? CaseSessionActId { get; set; }
        public string NotificationTypeLabel { get; set; }
        public int? NotificationTypeId { get; set; }
        public string RegNumber { get; set; }
        public string CasePersonName { get; set; }
        public string NotificationStateLabel { get; set; }
        public string HtmlTemplateLabel { get; set; }
        public int? NotificationNumber { get; set; }
        public DateTime RegDate { get; set; }
    }
}
