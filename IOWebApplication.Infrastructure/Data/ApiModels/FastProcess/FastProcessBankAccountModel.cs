// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace IOWebApplication.Infrastructure.Data.ApiModels.FastProcess
{
    /// <summary>
    /// Описание на начини на плащане и банкови сметки
    /// </summary>
    public class FastProcessBankAccountModel
    {
        [JsonProperty("bankAccountType")]
        public string BankAccountType { get; set; }

        [JsonProperty("iban")]
        public string IBAN { get; set; }

        [JsonProperty("bic")]
        public string BIC { get; set; }

        [JsonProperty("bankName")]
        public string BankName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
