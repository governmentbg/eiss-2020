using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class ReplaceDismissalRequest
    {
        [JsonProperty("dismissalId")]
        [Required]
        public Guid DismissalId { get; set; }

        [Required]
        public JudgeModel ReplaceJudge { get; set; }
    }
}
