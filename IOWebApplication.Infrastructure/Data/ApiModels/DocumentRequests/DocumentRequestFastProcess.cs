// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.ApiModels.Contracts;
using IOWebApplication.Infrastructure.Data.ApiModels.Doc;
using IOWebApplication.Infrastructure.Data.ApiModels.FastProcess;

namespace IOWebApplication.Infrastructure.Data.ApiModels.DocumentRequests
{
    public class DocumentRequestFastProcess : IDocumentRequest
    {
        public DocumentModel Document { get; set; }
        public FastProcessModel Data { get; set; }
    }
}
