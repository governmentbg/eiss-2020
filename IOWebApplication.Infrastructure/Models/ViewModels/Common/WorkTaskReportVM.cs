using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class WorkTaskReportVM
    {
        public long Id { get; set; }
        public int TaskTypeId { get; set; }
        public string TaskTypeName { get; set; }
        public string Description { get; set; }
        public string DescriptionCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateCompleted { get; set; }
    }
}
