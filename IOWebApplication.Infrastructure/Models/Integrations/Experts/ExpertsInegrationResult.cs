// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace IOWebApplication.Infrastructure.Models.Integrations.Experts
{
    public class ExpertsInegrationResult
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public Datum[] data { get; set; }
    }
}

public class Datum
{
    public string id { get; set; }
    public string reference { get; set; }

    [JsonProperty("full-name")]
    public string fullname { get; set; }
    public string competence { get; set; }

    [JsonProperty("pool-membership")]
    public string poolmembership { get; set; }
}
