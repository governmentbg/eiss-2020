// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Documents;
using System.Linq;

namespace IOWebApplication.Core.Contracts
{
    public interface IDocumentResolutionService : IBaseService
    {
        IQueryable<DocumentResolutionVM> Select(DocumentResolutionFilterVM filter);
        IQueryable<DocumentResolutionVM> Select(long documentId, long? id = null);
        SaveResultVM SaveData(DocumentResolution model);
        SaveResultVM Register(DocumentResolution model);
    }
}
