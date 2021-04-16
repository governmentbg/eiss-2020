using IOWebApplication.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkTaskVM
    {
        public long Id { get; set; }
        public string SourceInfo { get; set; }
        public string SourceDescription { get; set; }
        public string ParentDescription { get; set; }
        public int SourceType { get; set; }
        public long SourceId { get; set; }
        public long? SubSourceId { get; set; }
        public DateTime DateCreated { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanRedirect { get; set; }
        public bool CanAccept { get; set; }
        public DateTime? DateAccepted { get; set; }
        public bool CanProcess { get; set; }
        public bool CanComplete { get; set; }
        public bool CanCancel { get; set; }
        public string ViewUrl { get; set; }
        public bool CanDoAction { get; set; }
        public string DoActionUrl { get; set; }
        public string DoActionWarning { get; set; }
        public DateTime? DateEnd { get; set; }
        public bool OverDue { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string Description { get; set; }
        public string DescriptionCreated { get; set; }
        public int TaskTypeId { get; set; }
        public string TaskTypeName { get; set; }
        public int? TaskExecutionId { get; set; }
        public string UserId { get; set; }
        public int? CourtOrganizationId { get; set; }
        public string UserFullName { get; set; }
        public string UserCreatedId { get; set; }
        public string UserCreatedFullName { get; set; }
        public int TaskStateId { get; set; }
        public string TaskStateName { get; set; }
        public int? TaskActionId { get; set; }
        public string TaskActionName { get; set; }
        public bool CanCheckForManage
        {
            get
            {
                return WorkTaskConstants.States.NotFinished.Contains(this.TaskStateId);
            }
        }
    }
}
