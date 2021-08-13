// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace IOWebApplication.Infrastructure.Data.ApiModels.Doc
{
    public class DocumentPersonModel
    {
        //Unique GUID
        [JsonProperty("documentPersonId")]
        public string DocumentPersonId { get; set; }

        [JsonProperty("uic")]
        public string Uic { get; set; }
        //Nom
        [JsonProperty("uicType")]
        public string UicType { get; set; }

        [JsonProperty("entityName")]
        public string EntityName { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("middleName")]
        public string MiddleName { get; set; }

        [JsonProperty("familyName")]
        public string FamilyName { get; set; }

        [JsonProperty("family2Name")]
        public string Family2Name { get; set; }

        //Nom
        [JsonProperty("personRole")]
        public string PersonRole { get; set; }

        [JsonProperty("adresses")]
        public DocumentPersonAddressModel[] Addresses { get; set; }
    }
}
