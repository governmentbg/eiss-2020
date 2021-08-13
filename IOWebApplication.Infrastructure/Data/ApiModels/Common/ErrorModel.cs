// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0
using Newtonsoft.Json;


namespace IOWebApplication.Infrastructure.Data.ApiModels.Common
{
    public class ErrorModel
    {
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("errorDescription")]
        public string ErrorDescription { get; set; }
    }
}
