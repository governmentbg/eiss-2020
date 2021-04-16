using Newtonsoft.Json;

namespace IOWebApplication.Core.Models
{
    public class NomenclatureDisplayItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
