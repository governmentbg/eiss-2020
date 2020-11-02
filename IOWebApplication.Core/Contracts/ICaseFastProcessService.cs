// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using System.Collections.Generic;

namespace IOWebApplication.Core.Contracts
{
    public interface ICaseFastProcessService : IBaseService
    {
        CaseFastProcessViewVM Select(int CaseId);
        int CaseMoneyClaim_SaveData(CaseMoneyClaim model);
        int CaseMoneyCollection_SaveData(CaseMoneyCollectionEditVM model);
        List<CasePersonListDecimalVM> FillPersonList(int caseId, int? moneyCollectionId);
        List<CasePersonListDecimalVM> FillPersonListExpense(int caseId, int? caseMoneyExpenseId);
        int CaseMoneyExpense_SaveData(CaseMoneyExpenseEditVM model);
        int CaseBankAccount_SaveData(CaseBankAccount model);
        CaseMoneyCollectionEditVM GetById_EditVM(int Id);
        CaseMoneyExpenseEditVM GetById_ExpenseEditVM(int Id);
        List<CaseMoneyCollectionRespectSumVM> FillCaseMoneyCollectionRespectSum(int caseId);
        bool CaseMoneyCollectionRespectSum_SaveData(List<CaseMoneyCollectionRespectSumVM> model);
        bool CaseBankAccount_DeleteData(int Id);
        bool CaseMoneyExpense_DeleteData(int Id);
        bool CaseMoneyCollection_DeleteData(int Id);
        bool CaseMoneyClaim_DeleteData(int Id);
        CaseFastProcessVM CaseFastProcess_Select(int CaseId);
        CaseFastProcessEditVM GetByCaseId_CaseFastProcess(int CaseId);
        bool CaseFastProcess_SaveData(CaseFastProcessEditVM model);
        CaseBankAccount CaseBankAccount_GetLastByPerson(int CasePersonId, int? CaseBankAccountId);
    }
}
