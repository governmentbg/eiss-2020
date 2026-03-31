// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Threading.Tasks;

namespace IOWebApplicationService.Infrastructure.Contracts
{
    public interface IPdfCreatorService
    {
        Task<byte[]> CreatePDF(string html);
        Task<byte[]> RenderViewAsPDF(string viewPath, object model);
        Task<string> RenderViewAsText(string viewPath, object model);
    }
}
