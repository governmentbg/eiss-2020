// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.ApiModels.Common;
using IOWebApplication.Infrastructure.Data.ApiModels.Contracts;
using IOWebApplication.Infrastructure.Data.ApiModels.DocumentRequests;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IApiDocumentService
    {
        Task<DocumentResponseModel> RegisterDocumentAsync(IDocumentRequest model);


        Task<bool> UpdateFastProcessFromData(int caseId);

        DocumentVM TestDocument();

        (DocumentRequestFastProcess, DocumentVM) GenerateDocumentWithRequest();
        (DocumentRequestFastProcess, DocumentVM) GenerateDocumentWithRequestFromString(string json);
        void TestApi();
    }
}
