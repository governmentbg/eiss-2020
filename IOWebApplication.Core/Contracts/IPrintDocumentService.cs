using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface IPrintDocumentService : IBaseService
    {
        /// <summary>
        /// Управление на попълването на номер и дата на документа за preview на бланка.
        /// </summary>
        bool PreviewModel { get; set; }

        Task<(TinyMCEVM, CaseNotification)> FillHtmlTemplateNotification(int caseNotificationId);
        TinyMCEVM ConvertToTinyMCVM(HtmlTemplate htmlTemplate, bool insertDispositiv, string preparedBlank = null);
        TinyMCEVM FillHtmlTemplatePayment(int paymentId);
        Task<TinyMCEVM> FillHtmlTemplateNotificationTest(int caseNotificationId);

        /// <summary>
        /// Метод създаващ файл за разходен ордер
        /// </summary>
        /// <param name="orderId">Идентификатор на ордера</param>
        /// <returns></returns>
        (TinyMCEVM result, string errorMessage) FillHtmlTemplateExpenseOrder(int orderId);

        Task<TinyMCEVM> FillHtmlTemplateDocumentTemplate(int id, string preparedBlank = null);
        TinyMCEVM FillHtmlTemplateCaseSessionActDivorce(int divorceId);
        Task<TinyMCEVM> FillHtmlTemplateExecList(int execListId);
        TinyMCEVM FillHtmlTemplateSentenceBulletin(int id);
        Task FillHtmlTemplateNotificationHaveSaveTest(int caseNotificationId);
        TinyMCEVM FillHtmlTemplateExchangeDoc(int id);
        Task<TinyMCEVM> FillHtmlTemplateNotificationTestOne(int caseNotificationId, int htmlTemplateId);
        TinyMCEVM GetHtmlTemplateNull(int caseNotificationId, int documentNotificationId);
        void HtmlTemplateNotificationHave_F_FIRST_SET_NO_YEAR();
        IList<KeyValuePairVM> fillList_UpperCourt(int courtId, CaseSessionActComplain caseSessionActComplain);
        TinyMCEVM FillHtmlTemplateDocumentNotification(int documentNotificationId);
        void FillHtmlTemplate_F_DISPOSITIV();
        Task<TinyMCEVM> FillHtmlTemplateMediationNotification(int mediationNotificationId);

        /// <summary>
        /// Метод връщащ обект попълнен с данни за разпореждане за заповедно производство
        /// </summary>
        /// <param name="alias">Име на бланка</param>
        /// <param name="model">Модел с данни за акта за който се генерира бланка</param>
        /// <returns></returns>
        Task<TinyMCEVM> GetActFastProcess(string alias, CaseSessionActCommandVM model);
    }
}
