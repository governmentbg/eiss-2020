using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionMeetingVM
    {
        public int Id { get; set; }
        public int CaseSessionId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Description { get; set; }
        public string SessionMeetingTypeLabel { get; set; }
        public string UsersNames { get; set; }
        public string CourtHallLabel { get; set; }
        public string SecretaryUserNames { get; set; }
        public bool? IsAutoCreate { get; set; }
    }
}
