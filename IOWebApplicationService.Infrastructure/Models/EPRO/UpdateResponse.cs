using Newtonsoft.Json;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class UpdateResponse : BaseEproResponseModel
    {
        [JsonProperty("updateSuccessful")]
        public bool UpdateSuccessful { get; set; }
    }
}
