using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class DecisionModel
    {
        [Required]
        [JsonProperty("hearingType")]
        public string HearingType { get; set; }

        [Required]
        [JsonProperty("hearingDate")]
        public DateTime HearingDate { get; set; }

        [Required]
        [JsonProperty("actType")]
        public string ActType { get; set; }

        [Required]
        [JsonProperty("actNumber")]
        public int ActNumber { get; set; }

        [Required]
        [JsonProperty("actDeclaredDate")]
        public DateTime ActDeclaredDate { get; set; }
    }
}
