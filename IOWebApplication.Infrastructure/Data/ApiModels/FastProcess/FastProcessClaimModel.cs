// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;
using System;

namespace IOWebApplication.Infrastructure.Data.ApiModels.FastProcess
{
    /// <summary>
    /// Обстоятелство към искане
    /// </summary>
    public class FastProcessClaimModel
    {

        [JsonProperty("claimGroup")]
        public string ClaimGroup { get; set; }

        [JsonProperty("claimType")]
        public string ClaimType { get; set; }

        [JsonProperty("claimNumber")]
        public string ClaimNumber { get; set; }

        [JsonProperty("claimDate")]
        public DateTime? ClaimDate { get; set; }

        [JsonProperty("partyNames")]
        public string PartyNames { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("motive")]
        public string Motive { get; set; }

        [JsonProperty("claimMoney")]
        public FastProcessClaimMoneyModel[] ClaimMoney { get; set; }
    }
}
