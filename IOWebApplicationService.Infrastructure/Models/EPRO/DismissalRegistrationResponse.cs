using Newtonsoft.Json;
using System;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class DismissalRegistrationResponse : BaseEproResponseModel
    {
        [JsonProperty("dismissalId")]
        public Guid? DismissalId { get; set; }
    }
}
