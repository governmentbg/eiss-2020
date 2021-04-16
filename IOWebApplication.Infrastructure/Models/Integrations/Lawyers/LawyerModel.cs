using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.Integrations.Lawyers
{
    public class LawyerModel
    {
        [JsonProperty("personal_id")]
        public string PersonalId { get; set; }

        [JsonProperty("past_personal_id")]
        public string[] PastPersonalId { get; set; }

        [JsonProperty("bar_association")]
        public string BarAssociation { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("id_card_id")]
        public string IdCardId { get; set; }

        [JsonProperty("id_card_date")]
        public DateTime? IdCardDate { get; set; }

        [JsonProperty("e_mail")]
        public string Email { get; set; }

        [JsonProperty("is_junior")]
        public bool IsJunior { get; set; }

        [JsonProperty("practice_allowed")]
        public bool PracticeAllowed { get; set; }
    }
}
