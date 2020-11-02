// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IPrintDocumentService : IBaseService
    {
        TinyMCEVM FillHtmlTemplateNotification(int caseNotificationId);
        TinyMCEVM ConvertToTinyMCVM(HtmlTemplate htmlTemplate, string preparedBlank = null);
        TinyMCEVM FillHtmlTemplatePayment(int paymentId);
        TinyMCEVM FillHtmlTemplateNotificationTest(int caseNotificationId);
        TinyMCEVM FillHtmlTemplateExpenseOrder(int orderId);
        TinyMCEVM FillHtmlTemplateDocumentTemplate(int id, string preparedBlank = null);
        TinyMCEVM FillHtmlTemplateCaseSessionActDivorce(int divorceId);
        TinyMCEVM FillHtmlTemplateExecList(int execListId);
        TinyMCEVM FillHtmlTemplateSentenceBulletin(int id);
        void FillHtmlTemplateNotificationHaveSaveTest(int caseNotificationId);
        TinyMCEVM FillHtmlTemplateExchangeDoc(int id);
        TinyMCEVM FillHtmlTemplateNotificationTestOne(int caseNotificationId, int htmlTemplateId);
    }
}
