using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.AuditLog
{
    public class AuditLogSprVM
    {
        public DateTime Date { get; set; }
        public string Operation { get; set; }
        public string CaseGroupLabel { get; set; }
        public string CaseNumberLabel { get; set; }
        public string ObjectType { get; set; }
        public string ObjectInfo { get; set; }
        public string BaseObject { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int? CourtId { get; set; }
        public string RequestUrl { get; set; }
    }
}
