// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Models;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IOWebApplication.Test.Mockups
{
    public class CounterServiceMock : ICounterService
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

        public CurrentContextModel GetCurrentContext(int sourceType, long? sourceId, string operation = "", object parentId = null)
        {
            throw new NotImplementedException();
        }

        public void InitAllCounters()
        {
            throw new NotImplementedException();
        }

        bool IBaseService.SaveExpireInfo<T>(ExpiredInfoVM model)
        {
            throw new NotImplementedException();
        }
    }
}
