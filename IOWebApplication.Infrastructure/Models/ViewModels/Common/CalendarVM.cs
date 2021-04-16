using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CalendarVM
    {
        public string title { get; set; }
        public string pop_title { get; set; }
        public string pop_content { get; set; }
        public string url { get; set; }
        public DateTime start { get; set; }
        public DateTime? end { get; set; }
        public long? id { get; set; }
        public string color { get; set; }
        public bool allDay { get; set; }

        [JsonIgnore]
        public int SourceType { get; set; }
        [JsonIgnore]
        public long SourceId { get; set; }
    }
}
