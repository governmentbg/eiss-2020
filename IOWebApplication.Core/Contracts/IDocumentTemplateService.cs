// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using System.Collections.Generic;
using System.Linq;

namespace IOWebApplication.Core.Contracts
{
    public interface IDocumentTemplateService : IBaseService
    {
        DocumentTemplate DocumentTemplate_Init(int sourceType, long sourceId);
        IQueryable<DocumentTemplateVM> DocumentTemplate_Select(int sourceType, long sourceId);
        bool DocumentTemplate_SaveData(DocumentTemplate model);
        IEnumerable<LabelValueVM> DocumentTemplate_LoadBreadCrumbs(DocumentTemplate model);

        DocumentTemplateHeaderVM DocumentTemplate_InitHeader(int id);
        bool DocumentTemplate_UpdateDocumentId(int id, long documentId);
        DocumentTemplate DocumentTemplate_SelectByDocumentId(long documentId);
    }
}
