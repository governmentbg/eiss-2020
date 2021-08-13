using IOWebApplication.Infrastructure.Data.Models.Cases;
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
        TinyMCEVM ConvertToTinyMCVM(HtmlTemplate htmlTemplate, bool insertDispositiv, string preparedBlank = null);
        TinyMCEVM FillHtmlTemplatePayment(int paymentId);
        TinyMCEVM FillHtmlTemplateNotificationTest(int caseNotificationId);
        (TinyMCEVM result, string errorMessage) FillHtmlTemplateExpenseOrder(int orderId);
        TinyMCEVM FillHtmlTemplateDocumentTemplate(int id, string preparedBlank = null);
        TinyMCEVM FillHtmlTemplateCaseSessionActDivorce(int divorceId);
        TinyMCEVM FillHtmlTemplateExecList(int execListId);
        TinyMCEVM FillHtmlTemplateSentenceBulletin(int id);
        void FillHtmlTemplateNotificationHaveSaveTest(int caseNotificationId);
        TinyMCEVM FillHtmlTemplateExchangeDoc(int id);
        TinyMCEVM FillHtmlTemplateNotificationTestOne(int caseNotificationId, int htmlTemplateId);
        TinyMCEVM GetHtmlTemplateNull(int caseNotificationId);
        void HtmlTemplateNotificationHave_F_FIRST_SET_NO_YEAR();
        IList<KeyValuePairVM> fillList_UpperCourt(int courtId, CaseSessionActComplain caseSessionActComplain);
        TinyMCEVM FillHtmlTemplateDocumentNotification(int documentNotificationId);
    }
}
