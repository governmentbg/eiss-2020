// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;


namespace IOWebApplication.Infrastructure.Data.ApiModels.Common
{
   public class DocumentRegistrationModel
    {
        [JsonProperty("documentNumber")]
        public int DocumentNumber { get; set; }

        [JsonProperty("documentYear")]
        public int DocumentYear { get; set; }
    }
}
