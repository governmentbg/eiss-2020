// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.ApiModels.Doc;

namespace IOWebApplication.Infrastructure.Data.ApiModels.Contracts
{
    public interface IDocumentRequest
    {
        DocumentModel Document { get; set; }
    }
}
