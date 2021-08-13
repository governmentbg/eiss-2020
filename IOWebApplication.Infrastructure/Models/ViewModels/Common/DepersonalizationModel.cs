using System.Collections.Generic;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class DepersonalizationModel
    {
        public string SubmitAction { get; set; }
        public string SourceId { get; set; }
        public int CaseId { get; set; }

        public string DocumentName { get; set; }

        [AllowHtml]
        public string DocumentContent { get; set; }

        public string CancelUrl { get; set; }
        public string SaveMode { get; set; }

        public IEnumerable<DepersonalizationHistoryItem> DepersonalizationHistory { get; set; }

        [AllowHtml]
        public string DepersonalizationNewItems { get; set; }

        public DepersonalizationModel()
        {
            DepersonalizationHistory = new List<DepersonalizationHistoryItem>();
        }
    }
}
