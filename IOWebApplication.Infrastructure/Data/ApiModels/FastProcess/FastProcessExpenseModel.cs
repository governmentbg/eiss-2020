// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.ApiModels.FastProcess
{
    public class FastProcessExpenseModel
    {
        [JsonProperty("expenseType")]
        public string ExpenseType { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isJointDistribution")]
        public bool IsJointDistribution { get; set; }

        [JsonProperty("distribution")]
        public FastProcessDistributionModel[] Distribution { get; set; }
    }
}
