using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class JudgeModel
    {
        [Required]
        [JsonProperty("judgeName")]
        public string JudgeName { get; set; }
        [JsonProperty("isChairman")]
        public bool IsChairman { get; set; }
    }
}
