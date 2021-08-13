using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class ErrorModel
    {
        [JsonProperty("errorType")]
        public string ErrorType { get; set; }
        [JsonProperty("reason")]
        public string Reason { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("faultyAttribute")]
        public string FaultyAttribute { get; set; }

        public string GetErrorDescription()
        {
            return $"{ErrorType} {Reason} (Поле {FaultyAttribute})";
        }
    }
}
