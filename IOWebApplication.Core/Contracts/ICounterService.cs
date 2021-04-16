using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface ICounterService : IBaseService
    {
        IQueryable<CounterVM> Counter_Select(int courtId, string label);

        CounterVM[] Counter_GetCurrentValues(int courtId);
        bool Counter_SetCurrentValues(CounterVM[] model);

        CounterEditVM Counter_GetById(int id);
        bool Counter_SaveData(CounterEditVM model);

        /// <summary>
        /// Номериране на документи
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Counter_GetDocumentCounter(Document model);

        GetCounterValueVM Counter_GetDocumentCounterMulti(int counterCount, int docDirection, int courtId);

        /// <summary>
        /// Номериране на дела
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Counter_GetCaseCounter(Case model, int? oldNumber = null, DateTime? oldDate = null);
        bool Counter_GetCaseArchiveCounter(CaseArchive model);

        /// <summary>
        /// Номериране на актове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Counter_GetActCounter(CaseSessionAct model, int caseGroupId, int courtId);

        /// <summary>
        /// Номериране на призовки и писма
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Counter_GetNotificationCounter(CaseNotification model, int courtId);
        bool Counter_GetEvidenceCounter(CaseEvidence model, int courtId);


        void InitAllCounters();

        bool Counter_GetObligationCounter(Obligation model);

        bool Counter_GetPaymentCounter(Payment model);
        bool Counter_GetExpenseOrderCounter(ExpenseOrder model);
        bool Counter_GetDocumentDecisionCounter(DocumentDecision model);
        bool Counter_GetDivorceCounter(CaseSessionActDivorce model, int courtId);
        bool Counter_GetExecListCounter(ExecList model);
        bool Counter_GetExchangeCounter(ExchangeDoc model);
        bool Counter_GetDocumentResolutionCounter(DocumentResolution model);
        string Counter_GetCaseEisppNumber(int courtId);
        string Counter_GetCrimeEisppNumber(int courtId);
    }
}
