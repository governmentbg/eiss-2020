// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Money;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Contracts
{
    public interface IMoneyService : IBaseService
    {
        IQueryable<ObligationVM> Obligation_Select(int caseSessionActId, long documentId, int caseSessionId, int courtId);

        (bool result, string errorMessage) Obligation_SaveData(ObligationEditVM model);

        ObligationEditVM Obligation_GetById(int id);

        IQueryable<ObligationForPayVM> ObligationForPay_Select(int courtId, ObligationForPayFilterVM model);

        decimal GetSumForPay(string ids);

        (bool result, string errorMessage) MakePayment(PaymentVM model);

        List<ObligationForPayVM> ObligationForPayByIds_Select(string ids);

        List<int> MoneyGroup_Select(string ids);

        IQueryable<PaymentListVM> Payment_Select(int courtId, PaymentFilterVM model);

        bool Payment_SaveData(PaymentVM model);

        PaymentVM Payment_GetById(int id);

        bool Payment_Storno(int id, ref string errorMessage);

        IQueryable<ObligationForPayVM> ObligationPaymentForPayment_Select(int paymentId);

        IQueryable<PaymentListVM> ObligationPaymentForObligation_Select(int obligationId);

        bool ObligationPayment_Storno(int id, ref string errorMessage);

        //Неразнесени плащания
        IEnumerable<LabelValueVM> GetBalancePayment(int courtId, string senderName, int moneyGroupId);

        LabelValueVM GetPaymentById(int id);

        PaymentListVM GetPaymentById_BalancePayment(int id);

        bool BalancePayment_SaveData(BalancePaymentVM model, ref string errorMessage);

        bool PosPaymentResult_SaveData(PosPaymentResult model);

        //Записани успешно в ПОС, но незаписани при нас
        IQueryable<PosPaymentResultListVM> UnsavedPosPayment_Select(int courtId);

        //Генериране на плащане от PosPaymentResult
        (bool result, string errorMessage, int paymentId) MakePosPaymentFromPosResult(int id);

        (bool result, string errorMessage) CalcEarningsJury(CaseSession caseSession, int courtId);
        IQueryable<Obligation> ObligationByIds_Select(string ids);
        (bool result, string errorMessage) ExpenseOrder_Save(ExpenseOrderEditVM model);
        IQueryable<ExpenseOrderVM> ExpenseOrder_Select(int courtId, DateTime? fromDate, DateTime? toDate, string name, string expenseOrderRegNumber);
        (bool result, string errorMessage) ExpenseOrder_Storno(int id); 
        (bool result, string errorMessage) ExpenseOrder_Update(ExpenseOrderEditVM model);
        ExpenseOrderEditVM ExpenseOrder_GetById(int id);
        ExpenseOrder ExpenseOrder_LastOrderForPerson(string obligationIdStr);
        (bool result, string errorMessage) ExecList_Save(ExecListEditVM model);
        IQueryable<ExecListVM> ExecList_Select(int courtId, ExecListFilterVM model);
        (bool result, string errorMessage) ExecList_Storno(int id);
        (bool result, string errorMessage) ExecList_Update(ExecListEditVM model);
        ExecListEditVM ExecList_GetById(int id);
        List<SelectListItem> CountryReceive_SelectForDropDownList(int caseId);
        ObligationReceive LastDataForReceive_Select(string receiveId);
        (bool result, string errorMessage) ExecList_PrepareSave(ExecListEditVM model);
        IQueryable<PaymentCaseVM> PaymentForCase_Select(int caseId);
        IQueryable<ExecListVM> ExecListForCase_Select(int caseId);
        (bool result, string errorMessage, int id) ExchangeDoc_Save(string execListIds);
        IQueryable<ExchangeDocVM> ExchangeDoc_Select(int courtId, ExchangeDocFilterVM model);
        (bool result, string errorMessage) ExchangeDoc_Storno(int id);
        ExchangeDocEditVM ExchangeDoc_GetById(int id);
        IQueryable<ExecListVM> ExecListReport_Select(int courtId, ExecListFilterVM model, string newLine);
        byte[] ExecListReportToExcelOne(ExecListFilterVM model);
        IQueryable<ObligationThirdPersonVM> ObligationThirdPerson_Select(int courtId, ObligationThirdPersonFilterVM model);
    }
}
