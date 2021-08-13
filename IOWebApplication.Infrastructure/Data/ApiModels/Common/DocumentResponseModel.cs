// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace IOWebApplication.Infrastructure.Data.ApiModels.Common
{
    public class DocumentResponseModel
    {
        [JsonProperty("registeredSuccessful")]
        public bool RegisteredSuccessful { get; set; }

        [JsonProperty("registrationInfo")]
        public DocumentRegistrationModel RegistrationInfo { get; set; }

        [JsonProperty("error")]
        public ErrorModel Error { get; set; }
    }
}
