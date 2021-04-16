using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionElectronicFolderVM
    {
        public int Id { get; set; }
        public bool IsOnlyFiles { get; set; }
        public string SessionTypeLabel { get; set; }
        public string CourtHallName { get; set; }
        public string SessionStateLabel { get; set; }
        public string Description { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int DateTo_Minutes { get; set; }
        public string SessionStateString { get; set; }
        public string JudgeRapporteur { get; set; }
        public string Prokuror { get; set; }
        public string SessionResultText { get; set; }
        public virtual ICollection<CaseSessionNotificationListVM> CaseSessionNotificationLists { get; set; }
        public virtual ICollection<CaseSessionActVM> CaseSessionActs { get; set; }
        public virtual ICollection<CaseNotificationVM> CaseNotifications { get; set; }
        public virtual ICollection<CaseSessionMeetingVM> SessionMeetings { get; set; }
        public virtual ICollection<CaseSessionFastDocumentVM> SessionFastDocuments { get; set; }
        public virtual ICollection<CaseSessionDocVM> CaseSessionDocs { get; set; }
    }
}
