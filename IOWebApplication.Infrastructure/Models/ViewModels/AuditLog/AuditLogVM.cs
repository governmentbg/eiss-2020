using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.AuditLog
{
    public class AuditLogVM
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EventType { get; set; }
        public virtual AuditLogContextInfoModelVM currentContext { get; set; }
        public virtual AuditLogActionVM Action { get; set; }
    }
}
