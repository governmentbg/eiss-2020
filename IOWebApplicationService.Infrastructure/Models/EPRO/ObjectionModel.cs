using Newtonsoft.Json;
using System;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class ObjectionModel
    {
        [JsonProperty("documentType")]
        public string DocumentType { get; set; }

        [JsonProperty("documentNumber")]
        public int DocumentNumber { get; set; }

        [JsonProperty("documentDate")]
        public DateTime DocumentDate { get; set; }

        [JsonProperty("sideName")]
        public string SideName { get; set; }

        [JsonProperty("sideInvolmentKind")]
        public string SideInvolmentKind { get; set; }
    }
}
