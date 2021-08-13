// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;
using System;

namespace IOWebApplication.Infrastructure.Data.ApiModels.FastProcess
{
    /// <summary>
    /// Вземане към обстоятелство
    /// </summary>
    public class FastProcessClaimMoneyModel
    {

        [JsonProperty("claimMoneyId")]
        public string ClaimMoneyId { get; set; }

        [JsonProperty("mainClaimMoneyId")]
        public string MainClaimMoneyId { get; set; }       

        /// <summary>
        /// Основен вид на вземането: Парично вземане, предаване на заместими вещи
        /// </summary>
        [JsonProperty("collectionGroup")]
        public string CollectionGroup { get; set; }

        /// <summary>
        /// Точен вид на вземането: Парично вземане: главница, друг вид вземане, Предавания на движима вещ, 
        /// </summary>
        [JsonProperty("collectionType")]
        public string CollectionType { get; set; }

        /// <summary>
        /// Вид допълнително парично вземане: лихва, такса, друго
        /// </summary>
        [JsonProperty("collectionKind")]
        public string CollectionKind { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("initial_amount")]
        public decimal InitialAmount { get; set; }

        [JsonProperty("pretended_amount")]
        public decimal PretendedAmount { get; set; }       

        [JsonProperty("dateFrom")]
        public DateTime? DateFrom { get; set; }

        [JsonProperty("dateToType")]
        public string DateToType { get; set; }

        [JsonProperty("dateTo")]
        public DateTime? DateTo { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("motive")]
        public string Motive { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("isJointDistribution")]
        public bool IsJointDistribution { get; set; }

        [JsonProperty("isFraction")]
        public bool IsFraction { get; set; }

        [JsonProperty("distribution")]
        public FastProcessDistributionModel[] Distribution { get; set; }
    }
}
