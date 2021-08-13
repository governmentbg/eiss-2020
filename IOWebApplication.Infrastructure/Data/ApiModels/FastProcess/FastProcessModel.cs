// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;


namespace IOWebApplication.Infrastructure.Data.ApiModels.FastProcess
{
    public class FastProcessModel
    {
        //Начини на плащане
        [JsonProperty("bankAccount")]
        public FastProcessBankAccountModel[] BankAccounts { get; set; }

        //Обстоятелства
        [JsonProperty("moneyClaims")]
        public FastProcessClaimModel[] MoneyClaims { get; set; }

        //Претендирани разноски
        [JsonProperty("expenses")]
        public FastProcessExpenseModel[] Exprenses { get; set; }

        // Държавна такса
        [JsonProperty("tax_amount")]
        public decimal TaxAmount { get; set; }

        // Валута
        [JsonProperty("currency")]
        public string Currency { get; set; }

        //Допълнителна информация
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
