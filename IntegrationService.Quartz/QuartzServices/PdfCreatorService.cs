// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IO.HtmlToPdf.Contracts;
using IOWebApplicationService.Infrastructure.Contracts;
using Razor.Templating.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntegrationService.Quartz.QuartzServices
{
    public class PdfCreatorService : IPdfCreatorService
    {
        private readonly IRazorTemplateEngine razorTemplateEngine;
        private readonly IIOHtmlToPdfService htmlToPdfService;
        public PdfCreatorService(
            IRazorTemplateEngine _razorTemplateEngine,
            IIOHtmlToPdfService _htmlToPdfService)
        {
            razorTemplateEngine = _razorTemplateEngine;
            htmlToPdfService = _htmlToPdfService;
        }

        public async Task<string> RenderViewAsText(string viewPath, object model)
        {
            var viewData = new Dictionary<string, object>();
            var html = await razorTemplateEngine.RenderAsync(viewPath, model, viewData);

            return html;
        }

        public Task<byte[]> CreatePDF(string html)
        {
            return htmlToPdfService.ConvertHtmlToPdf(html, GetPrintPDFOptions());
        }

        public async Task<byte[]> RenderViewAsPDF(string viewPath, object model)
        {
            var html = await RenderViewAsText(viewPath, model);
            return await CreatePDF(html);
        }

        static IO.HtmlToPdf.Models.PDFOptions GetPrintPDFOptions()
        {
            return new IO.HtmlToPdf.Models.PDFOptions()
            {
                DisplayHeaderFooter = false,
                Landscape = false,
                Margin = new IO.HtmlToPdf.Models.PDFMargin()
                {
                    Bottom = "1cm",
                    Top = "1.5cm",
                    Left = "1cm",
                    Right = "1cm"
                },
                Timeout = 0
            };
        }
    }
}
