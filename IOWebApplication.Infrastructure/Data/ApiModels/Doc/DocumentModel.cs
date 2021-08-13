// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Newtonsoft.Json;

namespace IOWebApplication.Infrastructure.Data.ApiModels.Doc
{
    public class DocumentModel
    {
        //Unique GUID
        [JsonProperty("documentId")]
        public string DocumentId { get; set; }

        //Court PAS code,3digit
        [JsonProperty("courtCode")]
        public string CourtCode { get; set; }

        //Document type PAS, ЕПЕП
        [JsonProperty("documentType")]
        public string DocumentType { get; set; }

        //Case type PAS, ЕПЕП
        [JsonProperty("caseType")]
        public string CaseType { get; set; }

        //Case code PAS, ЕПЕП
        [JsonProperty("caseCode")]
        public string CaseCode { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("senderCode")]
        public string SenderCode { get; set; }

        [JsonProperty("senderName")]
        public string SenderName { get; set; }

        [JsonProperty("documentPersons")]
        public DocumentPersonModel[] DocumentPersons { get; set; }
    }
}
