using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services.Mocks
{
    public class MockCounterService : ICounterService
    {
        public bool ChangeOrder<T>(object id, bool moveUp, Func<T, int?> orderProp, Expression<Func<T, int?>> setterProp, Expression<Func<T, bool>> predicate = null) where T : class
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetActCounter(CaseSessionAct model, int caseGroupId, int courtId)
        {
            throw new NotImplementedException();
        }

        public CounterEditVM Counter_GetById(int id)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetCaseArchiveCounter(CaseArchive model)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetCaseCounter(Case model, int? oldNumber = null, DateTime? oldDate = null)
        {
            throw new NotImplementedException();
        }

        public string Counter_GetCaseEisppNumber(int courtId)
        {
            throw new NotImplementedException();
        }

        public string Counter_GetCrimeEisppNumber(int courtId)
        {
            throw new NotImplementedException();
        }

        public CounterVM[] Counter_GetCurrentValues(int courtId)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetDivorceCounter(CaseSessionActDivorce model, int courtId)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetDocumentCounter(Document model)
        {
            throw new NotImplementedException();
        }

        public GetCounterValueVM Counter_GetDocumentCounterMulti(int counterCount, int docDirection, int courtId)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetDocumentDecisionCounter(DocumentDecision model)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetDocumentResolutionCounter(DocumentResolution model)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetEvidenceCounter(CaseEvidence model, int courtId)
        {
            model.RegNumberValue = 100;
            model.RegNumber = "100";

            return true;
        }

        public bool Counter_GetExchangeCounter(ExchangeDoc model)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetExecListCounter(ExecList model)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetExpenseOrderCounter(ExpenseOrder model)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetNotificationCounter(CaseNotification model, int courtId)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetObligationCounter(Obligation model)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetPaymentCounter(Payment model)
        {
            throw new NotImplementedException();
        }

        public bool Counter_SaveData(CounterEditVM model)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CounterVM> Counter_Select(int courtId, string label)
        {
            throw new NotImplementedException();
        }

        public bool Counter_SetCurrentValues(CounterVM[] mode)
        {
            throw new NotImplementedException();
        }

        public T GetById<T>(object id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCaseInfoById(int id)
        {
            return Task.FromResult(string.Empty);
        }

        public CurrentContextModel GetCurrentContext(int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            throw new NotImplementedException();
        }

        public Tprop GetPropById<T, Tprop>(Expression<Func<T, bool>> where, Expression<Func<T, Tprop>> select) where T : class
        {
            throw new NotImplementedException();
        }

        public void InitAllCounters()
        {
            throw new NotImplementedException();
        }

        public SystemParam SystemParam_Select(string paramName)
        {
            throw new NotImplementedException();
        }

        public int[] SystemParam_SelectIntValues(string paramName)
        {
            throw new NotImplementedException();
        }

        public string SystemParam_SelectValue(string paramName)
        {
            throw new NotImplementedException();
        }

        public Task<string> SystemParamGetValue(string paramName)
        {
            return Task.FromResult(string.Empty);
        }

        bool ICounterService.Counter_GetNotificationCounter(DocumentNotification model, int courtId)
        {
            throw new NotImplementedException();
        }

        string IBaseService.GetNomCodeById<T>(int id)
        {
            throw new NotImplementedException();
        }

        string IBaseService.GetNomLabelById<T>(int id)
        {
            throw new NotImplementedException();
        }

        bool IBaseService.SaveExpireInfo<T>(ExpiredInfoVM model)
        {
            throw new NotImplementedException();
        }

        bool ICounterService.Counter_GetCasePersonInheritanceCounter(CasePersonInheritance model, int courtId)
        {
            throw new NotImplementedException();
        }

        public string[] SystemParam_SelectStringValues(string paramName)
        {
            throw new NotImplementedException();
        }

        T IBaseService.ReadById<T>(int id)
        {
            throw new NotImplementedException();
        }

        DateTime? IBaseService.GetFirstHistoryDate<T>(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckCaseFeature(int caseId, string featureName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckCaseFeature(CaseFeatureInfoVM caseInfo, string featureName)
        {
            throw new NotImplementedException();
        }

        public string GetUserIdByLawUnitId(int lawUnitId)
        {
            throw new NotImplementedException();
        }

        Task<T> IBaseService.ReadByIdAsync<T>(int id)
        {
            throw new NotImplementedException();
        }


        public Task<T> GetByIdAsync<T>(object id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<TProp> GetPropByIdAsync<T, TProp>(Expression<Func<T, bool>> where, Expression<Func<T, TProp>> select) where T : class
        {
            throw new NotImplementedException();
        }

        Task<T> IBaseService.ReadByIdAsync<T>(long id)
        {
            throw new NotImplementedException();
        }

        public Task<CurrentContextModel> GetCurrentContextAsync(int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            throw new NotImplementedException();
        }

        Task<T> IBaseService.GetReadonlyAsync<T>(long id)
        {
            throw new NotImplementedException();
        }

        Task<T> IBaseService.GetReadonlyAsync<T>(int id)
        {
            throw new NotImplementedException();
        }

        public void SetImpersonatedUser(string impersonatedUserId)
        {
            throw new NotImplementedException();
        }

        public IDbContextTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public void ClearEntityTracker()
        {
            throw new NotImplementedException();
        }

        public bool StopTrackingApplicationUser()
        {
            throw new NotImplementedException();
        }

        T IBaseService.GetReadonly<T>(long id)
        {
            throw new NotImplementedException();
        }

        T IBaseService.GetReadonly<T>(int id)
        {
            throw new NotImplementedException();
        }

        TProp IBaseService.GetPropById<T, TProp>(int id, Expression<Func<T, TProp>> select)
        {
            throw new NotImplementedException();
        }

        Task<TProp> IBaseService.GetPropByIdAsync<T, TProp>(int id, Expression<Func<T, TProp>> select)
        {
            throw new NotImplementedException();
        }

        Task<TProp> IBaseService.GetPropByIdAsync<T, TProp>(long id, Expression<Func<T, TProp>> select)
        {
            throw new NotImplementedException();
        }

        TProp IBaseService.GetPropById<T, TProp>(long id, Expression<Func<T, TProp>> select)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetCaseBulletinFileCounter(CasePersonSentenceBulletinFile model, int courtId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetIntegrationKey(int integrationType, int sourceType, long sourceId)
        {
            throw new NotImplementedException();
        }

        public string PersonNamesBase_GeneratePersonGid(string savedGid = null)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime?> GetParamValueDate(string paramName, string defaultValue)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsNewExecProcessCase(int? caseId)
        {
            throw new NotImplementedException();
        }

        public bool Counter_GetNotificationCounter(MediationNotification model, int courtId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdByLawUnitIdAsync(int lawUnitId)
        {
            throw new NotImplementedException();
        }

        public void SetImpersonatedCourt(int impersonatedCourtId)
        {
            throw new NotImplementedException();
        }
    }
}
