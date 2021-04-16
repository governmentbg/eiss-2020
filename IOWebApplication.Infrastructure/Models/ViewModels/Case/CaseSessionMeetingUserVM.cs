using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionMeetingUserVM
    {
        public int Id { get; set; }
        public string SecretaryUserName { get; set; }
        public DateTime DateWrt { get; set; }
    }
}
