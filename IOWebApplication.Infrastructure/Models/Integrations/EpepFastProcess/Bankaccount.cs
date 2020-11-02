// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepFastProcess
{
    public class Bankaccount
    {
        public string BankName { get; set; }
        public string IBAN { get; set; }
        public string BIC { get; set; }
        public string Country { get; set; }
        public string AccountHolder { get; set; }
        public string Description { get; set; }
    }
}
