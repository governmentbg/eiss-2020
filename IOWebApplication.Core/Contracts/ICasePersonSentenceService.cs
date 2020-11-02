// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Contracts
{
    public interface ICasePersonSentenceService: IBaseService
    {
        IQueryable<CasePersonSentenceVM> CasePersonSentence_Select(int CasePersonId);
        bool CasePersonSentence_SaveData(CasePersonSentenceEditVM model);
        CasePersonSentenceEditVM CasePersonSentence_GetById(int id);
        List<CheckListVM> FillLawBase();
        IQueryable<CaseCrimeVM> CaseCrime_Select(int CaseId);
        bool CaseCrime_SaveData(CaseCrime model);
        CaseCrimeVM CaseCrime_GetById(int id);
        IQueryable<CasePersonCrimeVM> CasePersonCrime_Select(int CaseCrimeId);
        bool CasePersonCrime_SaveData(CasePersonCrime model);
        bool IsExistPersonCasePersonCrime(int CaseCrimeId, int CasePersonId);
        Task<bool> CasePersonCrimeFillFromEispp_SaveData(int caseId, string pnenmr);
        List<SelectListItem> GetDropDownList_CasePersonCrime(int caseId, bool addDefaultElement = true, bool addAllElement = false);
        IQueryable<CasePersonSentencePunishmentVM> CasePersonSentencePunishment_Select(int CasePersonSentenceId);
        bool CasePersonSentencePunishment_SaveData(CasePersonSentencePunishment model);
        CasePersonSentencePunishmentVM CasePersonSentencePunishment_GetById(int id);
        IQueryable<CasePersonSentencePunishmentCrimeVM> CasePersonSentencePunishmentCrime_Select(int CasePersonSentencePunishmentId);
        bool CasePersonSentencePunishmentCrime_SaveData(CasePersonSentencePunishmentCrime model);
        List<SelectListItem> GetDropDownList_CasePersonSentence(int CasePersonId, int ModelId, bool addDefaultElement = true, bool addAllElement = false);
        CasePersonSentenceBulletin CasePersonSentenceBulletin_GetByIdPerson(int personId);
        CasePersonSentenceBulletinEditVM CasePersonSentenceBulletin_GetById(int id);
        (bool result, string errorMessage) CasePersonSentenceBulletin_SaveData(CasePersonSentenceBulletinEditVM model);
        CasePersonSentence CasePersonSentence_GetByPerson(int personId);
    }
}
