// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace IOWebApplication.Infrastructure.Data.ApiModels.FastProcess
{
    public class FastProcessDistributionModel
    {
        [JsonProperty("documentPersonId")]
        public string DocumentPersonId { get; set; }

        [JsonProperty("amountNumerator")]
        public int? AmountNumerator { get; set; }

        [JsonProperty("amountDenominator")]
        public int? AmountDenominator { get; set; }

        [JsonProperty("personAmount")]
        public decimal? PersonAmount { get; set; }
    }
}
