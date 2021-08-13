using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplicationService.Infrastructure.Models.EPRO
{
    public class ActPublicationRequest
    {
        [JsonProperty("dismissalId")]
        [Required]
        public Guid DismissalId { get; set; }

        [JsonProperty("fileName")]
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// Base64 encoded byte[]
        /// </summary>
        [JsonProperty("fileSource")]
        [Required]
        public string FileSource { get; set; }
        [JsonProperty("mimeType")]
        [Required]
        public string MimeType { get; set; }
    }
}
