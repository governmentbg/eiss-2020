using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class DismissalRegistrationRequest
    {
        //[Required] on update
        [JsonProperty("dismissalId")]
        public Guid? DismissalId { get; set; }

        [Required]
        [JsonProperty("court")]
        public string Court { get; set; }
        [Required]
        [JsonProperty("caseType")]
        public string CaseType { get; set; }
        [Required]
        [JsonProperty("caseNumber")]
        public string CaseNumber { get; set; }
        [Required]
        [JsonProperty("caseYear")]
        public int CaseYear { get; set; }
        [Required]
        [JsonProperty("dismissalType")]
        public string DismissalType { get; set; }
        [JsonProperty("objectionUpheld")]
        public bool ObjectionUpheld { get; set; }

        [Required]
        [JsonProperty("dismissalReason")]
        public string DismissalReason { get; set; }

        [Required]
        [JsonProperty("caseRole")]
        public string CaseRole { get; set; }


        [Required]
        public JudgeModel Judge { get; set; }
        public ObjectionModel Objection { get; set; }
        [Required]
        public DecisionModel Decision { get; set; }
    }
}
