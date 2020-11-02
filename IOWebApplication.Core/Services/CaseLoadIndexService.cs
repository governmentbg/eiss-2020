// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using IOWebApplication.Infrastructure.Extensions;
using ZXing;
using IOWebApplication.Core.Helper.GlobalConstants;
using Microsoft.Extensions.Configuration;
using IOWebApplication.Infrastructure.Data.Models.Identity;

namespace IOWebApplication.Core.Services
{
    public class CaseLoadIndexService : BaseService, ICaseLoadIndexService
    {
        private readonly IPriceService priceService;
        private readonly ICaseLoadCorrectionService caseLoadCorrectionService;
        IConfiguration configuration;

        public CaseLoadIndexService(
        ILogger<CaseLoadIndexService> _logger,
        IPriceService _priceService,
        IRepository _repo,
        IConfiguration _configuration,
        ICaseLoadCorrectionService _caseLoadCorrectionService,
        AutoMapper.IMapper _mapper,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            mapper = _mapper;
            userContext = _userContext;
            priceService = _priceService;
            configuration = _configuration;
            caseLoadCorrectionService = _caseLoadCorrectionService;
        }

        #region Case Load Index

        /// <summary>
        /// Извличане на данни за Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadIndexVM> CaseLoadIndex_Select(int CaseId, int? CaseSessionId)
        {
            return repo.AllReadonly<CaseLoadIndex>()
                .Include(x => x.LawUnit)
                .Include(x => x.CaseLoadElementGroup)
                .Include(x => x.CaseLoadElementType)
                .Include(x => x.CaseLoadAddActivity)
                .Where(x => x.CaseId == CaseId &&
                            (CaseSessionId != null ? x.CaseSessionId == CaseSessionId : true) &&
                            x.DateExpired == null)
                .Select(x => new CaseLoadIndexVM()
                {
                    Id = x.Id,
                    CaseId = x.CaseId,
                    CaseSessionId = x.CaseSessionId,
                    BaseIndex = x.BaseIndex,
                    LawUnitId = x.LawUnitId,
                    LawUnitName = x.LawUnit.FullName,
                    LoadValue = (x.IsMainActivity) ? x.LoadProcent.ToString("0.00") + "%" : x.LoadIndex.ToString("0.00"),
                    NameActivity = (x.IsMainActivity) ? x.CaseLoadElementGroup.Label + " - " + x.CaseLoadElementType.Label : x.CaseLoadAddActivity.Label,
                    CalcValue = (x.IsMainActivity) ? Math.Round(x.BaseIndex * (x.LoadProcent / 100), 3) : x.LoadIndex,
                    IsMainActivity = x.IsMainActivity
                })
                .AsQueryable();
        }

        /// <summary>
        /// Проверка за съществуване в дело на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="ModelId"></param>
        /// <param name="CaseId"></param>
        /// <param name="isMainActivity"></param>
        /// <param name="JudgeRepLawUnitId"></param>
        /// <param name="caseLoadElementTypeId"></param>
        /// <param name="caseLoadAddActivityId"></param>
        /// <returns></returns>
        public bool IsExistCaseLoadActivity(int ModelId, int CaseId, bool isMainActivity, int JudgeRepLawUnitId, int? caseLoadElementTypeId, int? caseLoadAddActivityId)
        {
            if (!isMainActivity)
            {
                return repo.AllReadonly<CaseLoadIndex>()
                           .Any(x => (x.CaseId == CaseId) &&
                                     ((ModelId > 0) ? x.Id != ModelId : true) &&
                                     (x.LawUnitId == JudgeRepLawUnitId) &&
                                     ((isMainActivity) ? x.CaseLoadElementTypeId == caseLoadElementTypeId : x.CaseLoadAddActivityId == caseLoadAddActivityId));
            }
            else
            {
                var caseLoadElementTypeIds = repo.AllReadonly<CaseLoadElementType>()
                                                 .Where(x => x.ReplaceCaseLoadElementTypeId == caseLoadElementTypeId)
                                                 .Select(x => x.Id)
                                                 .ToList() ?? new List<int>();

                if (caseLoadElementTypeIds.Count > 0)
                {
                    return repo.AllReadonly<CaseLoadIndex>()
                               .Any(x => (x.CaseId == CaseId) &&
                                         ((ModelId > 0) ? x.Id != ModelId : true) &&
                                         ((isMainActivity) ? (x.CaseLoadElementTypeId == caseLoadElementTypeId || caseLoadElementTypeIds.Contains(x.CaseLoadElementTypeId ?? 0)) && (x.LawUnitId == JudgeRepLawUnitId) : x.CaseLoadAddActivityId == caseLoadAddActivityId));
                }
                else
                {
                    return repo.AllReadonly<CaseLoadIndex>()
                           .Any(x => (x.CaseId == CaseId) &&
                                     ((ModelId > 0) ? x.Id != ModelId : true) &&
                                     ((isMainActivity) ? x.CaseLoadElementTypeId == caseLoadElementTypeId && x.LawUnitId == JudgeRepLawUnitId : x.CaseLoadAddActivityId == caseLoadAddActivityId));
                }
            }
        }

        private bool IsExistCaseLoadActivityInListSave(List<CaseLoadIndex> caseLoadIndicesSave, bool isMainActivity, int JudgeRepLawUnitId, int? caseLoadElementTypeId, int? caseLoadAddActivityId)
        {
            var caseLoadElementTypeIds = new List<int>();
            if (isMainActivity)
            {
                caseLoadElementTypeIds = repo.AllReadonly<CaseLoadElementType>()
                                             .Where(x => x.ReplaceCaseLoadElementTypeId == caseLoadElementTypeId)
                                             .Select(x => x.Id)
                                             .ToList() ?? new List<int>();
            }
            
            return caseLoadIndicesSave.Any(x => ((isMainActivity) ? (caseLoadElementTypeIds.Count > 0 ? (x.CaseLoadElementTypeId == caseLoadElementTypeId) : (x.CaseLoadElementTypeId == caseLoadElementTypeId || caseLoadElementTypeIds.Contains(x.CaseLoadElementTypeId ?? 0))) && (x.LawUnitId == JudgeRepLawUnitId) : x.CaseLoadAddActivityId == caseLoadAddActivityId));
        }

        /// <summary>
        /// Извличане на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="isMainActivity"></param>
        /// <param name="caseLoadElementTypeId"></param>
        /// <param name="caseLoadAddActivityId"></param>
        /// <param name="courtTypeId"></param>
        /// <returns></returns>
        private decimal GetLoadIndex_CaseLoadIndex(bool isMainActivity, int? caseLoadElementTypeId, int? caseLoadAddActivityId, int? courtTypeId)
        {
            if (isMainActivity)
            {
                var caseLoadElementType = repo.GetById<CaseLoadElementType>(caseLoadElementTypeId);
                return caseLoadElementType.LoadProcent;
            }
            else
            {


                var caseLoad = repo.AllReadonly<CaseLoadAddActivityIndex>().Where(x => x.CaseLoadAddActivityId == caseLoadAddActivityId && x.CourtTypeId == courtTypeId).FirstOrDefault();
                if (caseLoad != null)
                {
                    return caseLoad.LoadIndex;
                }
            }

            return 0;
        }

        /// <summary>
        /// Запис на Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadIndex_SaveData(CaseLoadIndex model)
        {
            try
            {
                model.DescriptionExpired = null;
                var caseCase = repo.AllReadonly<Case>()
                                   .Include(x => x.Court)
                                   .Where(x => x.Id == model.CaseId)
                                   .FirstOrDefault();

                model.CaseLoadElementGroupId = model.CaseLoadElementGroupId.EmptyToNull(0);
                model.CaseLoadElementTypeId = model.CaseLoadElementTypeId.EmptyToNull(0);
                model.CaseLoadAddActivityId = model.CaseLoadAddActivityId.EmptyToNull(0);

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadIndex>(model.Id);
                    saved.LawUnitId = model.LawUnitId;

                    if ((saved.IsMainActivity != model.IsMainActivity) ||
                        (saved.CaseLoadElementGroupId != model.CaseLoadElementGroupId) ||
                        (saved.CaseLoadElementTypeId != model.CaseLoadElementTypeId) ||
                        (saved.CaseLoadAddActivityId != model.CaseLoadAddActivityId))
                    {
                        if (model.IsMainActivity)
                            saved.LoadProcent = GetLoadIndex_CaseLoadIndex(model.IsMainActivity, model.CaseLoadElementTypeId, null, null);
                        else
                            saved.LoadIndex = GetLoadIndex_CaseLoadIndex(model.IsMainActivity, null, model.CaseLoadAddActivityId, caseCase.Court.CourtTypeId);

                        saved.IsMainActivity = model.IsMainActivity;
                        saved.CaseLoadElementGroupId = model.CaseLoadElementGroupId;
                        saved.CaseLoadElementTypeId = model.CaseLoadElementTypeId;
                        saved.CaseLoadAddActivityId = model.CaseLoadAddActivityId;
                        //saved.BaseIndex = model.BaseIndex;
                    }

                    saved.DateActivity = model.DateActivity;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    if (model.IsMainActivity)
                        model.LoadProcent = GetLoadIndex_CaseLoadIndex(model.IsMainActivity, model.CaseLoadElementTypeId, null, null);
                    else
                        model.LoadIndex = GetLoadIndex_CaseLoadIndex(model.IsMainActivity, null, model.CaseLoadAddActivityId, caseCase.Court.CourtTypeId);

                    var caseLoadCorrectionIdex = caseLoadCorrectionService.GetCaseLoadCorrection(model.CaseId);
                    model.BaseIndex = caseLoadCorrectionIdex > 0 ? caseCase.LoadIndex * caseLoadCorrectionIdex : caseCase.LoadIndex;
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseLoadIndex>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={ model.Id }");
                return false;
            }
        }

        private CaseLoadIndex FillCaseLoadIndex(List<CaseLoadIndex> caseLoadIndices, CaseSession caseSession, CaseSessionAct caseSessionAct, CaseSessionResult caseSessionResult, CaseLawUnit judgeRep, int CaseLoadElementGroupId, int CaseLoadElementTypeId, decimal LoadProcent, int? ReplaceCaseLoadElementTypeId, decimal CaseLoadCorrectionIndex)
        {
            var saveCaseLoadIndex = new CaseLoadIndex();

            if (ReplaceCaseLoadElementTypeId != null)
            {
                saveCaseLoadIndex = repo.AllReadonly<CaseLoadIndex>()
                                        .Where(x => x.CaseId == caseSession.CaseId &&
                                                    x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId)
                                        .FirstOrDefault() ?? new CaseLoadIndex();

                if (saveCaseLoadIndex.Id < 1)
                {
                    saveCaseLoadIndex = caseLoadIndices
                                        .Where(x => x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId)
                                        .FirstOrDefault() ?? new CaseLoadIndex();
                }
            }

            saveCaseLoadIndex.CourtId = caseSession.CourtId;
            saveCaseLoadIndex.CaseId = caseSession.CaseId;
            saveCaseLoadIndex.CaseSessionId = caseSession.Id;
            saveCaseLoadIndex.SessionTypeId = caseSession.SessionTypeId;
            saveCaseLoadIndex.CaseSessionActId = caseSessionAct != null ? caseSessionAct.Id : (int?)null;
            saveCaseLoadIndex.ActTypeId = caseSessionAct != null ? caseSessionAct.ActTypeId : (int?)null;
            saveCaseLoadIndex.CaseSessionResultId = caseSessionResult != null ? caseSessionResult.Id : (int?)null;
            saveCaseLoadIndex.SessionResultId = caseSessionResult != null ? caseSessionResult.SessionResultId : (int?)null;
            saveCaseLoadIndex.LawUnitId = judgeRep.LawUnitId;
            saveCaseLoadIndex.DateActivity = DateTime.Now;
            saveCaseLoadIndex.IsMainActivity = true;
            saveCaseLoadIndex.CaseLoadElementGroupId = CaseLoadElementGroupId;
            saveCaseLoadIndex.CaseLoadElementTypeId = CaseLoadElementTypeId;
            saveCaseLoadIndex.LoadProcent = LoadProcent;
            saveCaseLoadIndex.BaseIndex = (CaseLoadCorrectionIndex > 0) ? caseSession.Case.LoadIndex * CaseLoadCorrectionIndex : caseSession.Case.LoadIndex;
            saveCaseLoadIndex.DateWrt = DateTime.Now;
            saveCaseLoadIndex.UserId = userContext.UserId;

            return saveCaseLoadIndex;
        }

        private CaseLoadIndex FillCaseLoadIndex(List<CaseLoadIndex> caseLoadIndices, Case caseCase, CaseLawUnit judgeRep, int CaseLoadElementGroupId, int CaseLoadElementTypeId, decimal LoadProcent, int? ReplaceCaseLoadElementTypeId, decimal CaseLoadCorrectionIndex)
        {
            var saveCaseLoadIndex = new CaseLoadIndex();

            if (ReplaceCaseLoadElementTypeId != null)
            {
                saveCaseLoadIndex = repo.AllReadonly<CaseLoadIndex>()
                                        .Where(x => x.CaseId == caseCase.Id &&
                                                    x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId)
                                        .FirstOrDefault() ?? new CaseLoadIndex();

                if (saveCaseLoadIndex.Id < 1)
                {
                    saveCaseLoadIndex = caseLoadIndices
                                        .Where(x => x.CaseLoadElementTypeId == ReplaceCaseLoadElementTypeId)
                                        .FirstOrDefault() ?? new CaseLoadIndex();
                }
            }

            saveCaseLoadIndex.CourtId = caseCase.CourtId;
            saveCaseLoadIndex.CaseId = caseCase.Id;
            saveCaseLoadIndex.CaseSessionId = null;
            saveCaseLoadIndex.SessionTypeId = null;
            saveCaseLoadIndex.CaseSessionActId = null;
            saveCaseLoadIndex.ActTypeId = null;
            saveCaseLoadIndex.CaseSessionResultId = null;
            saveCaseLoadIndex.SessionResultId = null;
            saveCaseLoadIndex.LawUnitId = judgeRep.LawUnitId;
            saveCaseLoadIndex.DateActivity = DateTime.Now;
            saveCaseLoadIndex.IsMainActivity = true;
            saveCaseLoadIndex.CaseLoadElementGroupId = CaseLoadElementGroupId;
            saveCaseLoadIndex.CaseLoadElementTypeId = CaseLoadElementTypeId;
            saveCaseLoadIndex.LoadProcent = LoadProcent;
            saveCaseLoadIndex.BaseIndex = (CaseLoadCorrectionIndex > 0) ? caseCase.LoadIndex * CaseLoadCorrectionIndex : caseCase.LoadIndex;
            saveCaseLoadIndex.DateWrt = DateTime.Now;
            saveCaseLoadIndex.UserId = userContext.UserId;

            return saveCaseLoadIndex;
        }

        public bool CaseLoadIndexAutomationElementGroupe_SRA_SaveData(int CaseSessionId)
        {
            //var environmentName = configuration.GetValue<string>("Environment:Name") ?? NomenclatureConstants.Environments.Production;
            //if (environmentName == NomenclatureConstants.Environments.Production)
            //{
            //    return false;
            //}

            var caseSession = repo.AllReadonly<CaseSession>()
                              .Include(x => x.CaseSessionResults)
                              .Include(x => x.CaseSessionActs)
                              .Include(x => x.Case)
                              .ThenInclude(x => x.CaseLawUnits)
                              .Include(x => x.Case)
                              .ThenInclude(x => x.CaseType)
                              .Where(x => x.Id == CaseSessionId)
                              .FirstOrDefault();

            var judgeRep = caseSession.Case.CaseLawUnits.Where(x => (x.CaseSessionId == null) && 
                                                                    ((x.DateTo ?? caseSession.DateFrom.AddYears(100)).Date >= caseSession.DateFrom.Date) && 
                                                                    (x.DateFrom <= caseSession.DateTo) &&
                                                                    (x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter))
                                                        .FirstOrDefault();
            if (judgeRep == null)
                return false;

            var caseLoadElementGroups = ReadAllCaseLoadElementGroupByCaseNew(caseSession.CaseId);
            if (caseLoadElementGroups.Count < 1)
                return false;

            var caseLoadCorrectionIndex = caseLoadCorrectionService.GetCaseLoadCorrection(caseSession.CaseId);

            var caseLoadIndexSave = new List<CaseLoadIndex>();
            foreach (var caseLoad in caseLoadElementGroups)
            {
                foreach (var caseLoadElementType in caseLoad.CaseLoadElementTypes)
                {
                    foreach (var caseSessionResult in caseSession.CaseSessionResults.Where(x => x.DateExpired == null))
                    {
                        foreach (var caseSessionAct in caseSession.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActStateId != NomenclatureConstants.SessionActState.Project))
                        {
                            // Проверка за тип заседание/вид резултат/вид акт
                            if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                caseSessionResult.SessionResultId,
                                                                caseSessionAct.ActTypeId,
                                                                false,
                                                                false))
                            {
                                if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) && 
                                    !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                {
                                    caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, caseSessionResult, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex));
                                }
                            }

                            // Проверка за създаване на мотив
                            if (caseSessionAct.ActMotivesDeclaredDate != null)
                            {
                                // Проверка за тип заседание/вид резултат/вид акт
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                caseSessionResult.SessionResultId,
                                                                caseSessionAct.ActTypeId,
                                                                true,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                        !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, caseSessionResult, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex));
                                    }
                                }

                                // Проверка за тип заседание
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                null,
                                                                null,
                                                                true,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                        !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, null, null, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex));
                                    }
                                }

                                // Проверка за тип заседание/вид акт
                                if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                                caseSession.SessionTypeId,
                                                                null,
                                                                caseSessionAct.ActTypeId,
                                                                true,
                                                                false))
                                {
                                    if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                        !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                    {
                                        caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, null, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (caseLoadIndexSave.Count > 0)
            {
                try
                {
                    foreach (var loadIndex in caseLoadIndexSave)
                    {
                        if (loadIndex.Id > 0)
                            repo.Update(loadIndex);
                        else
                            repo.Add<CaseLoadIndex>(loadIndex);
                    }

                    repo.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={ caseSession.CaseId }");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Проверка за съществуващо правило по зададени критерии
        /// </summary>
        /// <param name="caseLoadElementTypeRules"></param>
        /// <param name="SessionTypeId"></param>
        /// <param name="SessionResultId"></param>
        /// <param name="ActTypeId"></param>
        /// <param name="IsCreateCase"></param>
        /// <param name="IsCreateMotive"></param>
        /// <returns></returns>
        private bool IsExistCaseLoadElementTypeRules(ICollection<CaseLoadElementTypeRule> caseLoadElementTypeRules, int? SessionTypeId, int? SessionResultId, int? ActTypeId, bool? IsCreateMotive, bool? IsCreateCase)
        {
            return caseLoadElementTypeRules.Any(x => x.SessionTypeId == SessionTypeId &&
                                                     x.SessionResultId == SessionResultId &&
                                                     x.ActTypeId == ActTypeId &&
                                                     x.IsCreateCase == IsCreateCase &&
                                                     x.IsCreateMotive == IsCreateMotive &&
                                                     x.DateExpired == null);
        }

        public bool CaseLoadIndexAutomationElementGroupe_CC_SaveData(int CaseId)
        {
            //var environmentName = configuration.GetValue<string>("Environment:Name") ?? NomenclatureConstants.Environments.Production;
            //if (environmentName == NomenclatureConstants.Environments.Production)
            //{
            //    return false;
            //}

            var caseCase = repo.AllReadonly<Case>()
                               .Include(x => x.CaseLawUnits)
                               .Include(x => x.CaseType)
                               .Include(x => x.CaseSessions)
                               .Where(x => x.Id == CaseId)
                               .FirstOrDefault();

            var caseSession = caseCase.CaseSessions.Where(x => x.DateExpired == null).OrderBy(x => x.DateFrom).FirstOrDefault();

            var judgeRep = caseCase.CaseLawUnits.Where(x => (x.CaseSessionId == null) &&
                                                            ((x.DateTo ?? caseSession.DateFrom.AddYears(100)).Date >= caseSession.DateFrom.Date) &&
                                                            (x.DateFrom <= caseSession.DateTo) &&
                                                            (x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter))
                                                .FirstOrDefault();


            if (judgeRep == null)
                return false;

            var caseLoadElementGroups = ReadAllCaseLoadElementGroupByCaseNew(CaseId);
            if (caseLoadElementGroups.Count < 1)
                return false;

            var caseLoadCorrectionIndex = caseLoadCorrectionService.GetCaseLoadCorrection(CaseId);

            var caseLoadIndexSave = new List<CaseLoadIndex>();
            foreach (var caseLoad in caseLoadElementGroups)
            {
                foreach (var caseLoadElementType in caseLoad.CaseLoadElementTypes)
                {
                    // Проверка за образуване на дело
                    if (IsExistCaseLoadElementTypeRules(caseLoadElementType.CaseLoadElementTypeRules,
                                                        null,
                                                        null,
                                                        null,
                                                        false,
                                                        true))
                    {
                        if (!IsExistCaseLoadActivity(0, CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                            !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                        {
                            caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseCase, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex));
                        }
                    }
                }
            }

            if (caseLoadIndexSave.Count > 0)
            {
                try
                {
                    foreach (var loadIndex in caseLoadIndexSave)
                    {
                        repo.Add<CaseLoadIndex>(loadIndex);
                    }

                    repo.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={ CaseId }");
                    return false;
                }
            }

            return false;
        }

        public bool CaseLoadIndexAutomationElementGroupeND_SaveData(int CaseSessionId)
        {
            var caseSession = repo.AllReadonly<CaseSession>()
                                  .Include(x => x.CaseSessionResults)
                                  .Include(x => x.CaseSessionActs)
                                  .Include(x => x.Case)
                                  .ThenInclude(x => x.CaseLawUnits)
                                  .Include(x => x.Case)
                                  .ThenInclude(x => x.CaseType)
                                  .Where(x => x.Id == CaseSessionId)
                                  .FirstOrDefault();

            if (caseSession.Case.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo)
                return false;

            var judgeRep = caseSession.Case.CaseLawUnits.Where(x => x.CaseSessionId == null && (x.DateTo ?? DateTime.Now.AddYears(100)).Date >= DateTime.Now.Date && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
            if (judgeRep == null)
                return false;

            var caseLoadElementGroups = ReadAllCaseLoadElementGroupByCase(caseSession.CaseId);
            if (caseLoadElementGroups.Count < 1)
                return false;

            var caseLoadCorrectionIndex = caseLoadCorrectionService.GetCaseLoadCorrection(caseSession.CaseId);

            var caseLoadIndexSave = new List<CaseLoadIndex>();
            foreach (var caseLoad in caseLoadElementGroups)
            {
                foreach (var caseLoadElementType in caseLoad.CaseLoadElementTypes)
                {
                    foreach (var caseSessionResult in caseSession.CaseSessionResults.Where(x => x.DateExpired == null))
                    {
                        foreach (var caseSessionAct in caseSession.CaseSessionActs.Where(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActStateId != NomenclatureConstants.SessionActState.Project))
                        {
                            if (caseLoadElementType.CaseLoadElementTypeRules.Any(x => x.SessionTypeId == caseSession.SessionTypeId &&
                                                                                      x.SessionResultId == caseSessionResult.SessionResultId &&
                                                                                      x.ActTypeId == caseSessionAct.ActTypeId &&
                                                                                      x.DateExpired == null))
                            {
                                if (!IsExistCaseLoadActivity(0, caseSession.CaseId, true, judgeRep.LawUnitId, caseLoadElementType.Id, null) &&
                                    !IsExistCaseLoadActivityInListSave(caseLoadIndexSave, true, judgeRep.LawUnitId, caseLoadElementType.Id, null))
                                {
                                    caseLoadIndexSave.Add(FillCaseLoadIndex(caseLoadIndexSave, caseSession, caseSessionAct, caseSessionResult, judgeRep, caseLoadElementType.CaseLoadElementGroupId, caseLoadElementType.Id, caseLoadElementType.LoadProcent, caseLoadElementType.ReplaceCaseLoadElementTypeId, caseLoadCorrectionIndex));
                                }
                            }
                        }
                    }
                }
            }

            if (caseLoadIndexSave.Count > 0)
            {
                try
                {
                    foreach (var loadIndex in caseLoadIndexSave)
                    {
                        repo.Add<CaseLoadIndex>(loadIndex);
                    }

                    repo.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на Натовареност по дела Id={ caseSession.CaseId }");
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Извличане на основни дейности по ид на дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseLoadElementGroup> ReadAllCaseLoadElementGroupByCase(int CaseId)
        {
            var caseCase = repo.AllReadonly<Case>()
                               .Include(x => x.Document)
                               .Include(x => x.CaseType)
                               .Where(x => x.Id == CaseId)
                               .FirstOrDefault();

            var result = new List<CaseLoadElementGroup>();
            var isND = (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo);

            result.AddRange(repo.All<CaseLoadElementGroup>()
                                .Include(x => x.CaseLoadElementTypes)
                                .ThenInclude(x => x.CaseLoadElementTypeRules)
                                .Where(x => (x.IsND == isND) &&
                                            (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                            (x.CaseTypeId == caseCase.CaseTypeId) &&
                                            (x.DocumentTypeId == null) &&
                                            (x.CaseCodeId == null) &&
                                            ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                .ToList() ?? new List<CaseLoadElementGroup>());

            result.AddRange(repo.All<CaseLoadElementGroup>()
                                .Include(x => x.CaseLoadElementTypes)
                                .ThenInclude(x => x.CaseLoadElementTypeRules)
                                .Where(x => (x.IsND == isND) &&
                                            (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                            (x.CaseTypeId == caseCase.CaseTypeId) &&
                                            (x.DocumentTypeId == caseCase.Document.DocumentTypeId) &&
                                            (x.CaseCodeId == null) &&
                                            ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                .ToList() ?? new List<CaseLoadElementGroup>());

            result.AddRange(repo.All<CaseLoadElementGroup>()
                                .Include(x => x.CaseLoadElementTypes)
                                .ThenInclude(x => x.CaseLoadElementTypeRules)
                                .Where(x => (x.IsND == isND) &&
                                            (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                            (x.CaseTypeId == caseCase.CaseTypeId) &&
                                            (x.DocumentTypeId == null) &&
                                            (x.CaseCodeId == caseCase.CaseCodeId) &&
                                            ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                .ToList() ?? new List<CaseLoadElementGroup>());

            result.AddRange(repo.All<CaseLoadElementGroup>()
                                .Include(x => x.CaseLoadElementTypes)
                                .ThenInclude(x => x.CaseLoadElementTypeRules)
                                .Where(x => (x.IsND == isND) &&
                                            (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                            (x.CaseTypeId == null) &&
                                            (x.DocumentTypeId == null) &&
                                            (x.CaseCodeId == null) &&
                                            ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                .ToList() ?? new List<CaseLoadElementGroup>());

            return result;
        }

        /// <summary>
        /// Извличане на основни дейности по ид на дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        private List<CaseLoadElementGroup> ReadAllCaseLoadElementGroupByCaseNew(int CaseId)
        {
            var caseCase = repo.AllReadonly<Case>()
                               .Include(x => x.Document)
                               .Include(x => x.CaseType)
                               .Where(x => x.Id == CaseId)
                               .FirstOrDefault();

            var result = new List<CaseLoadElementGroup>();
            var isND = (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo);

            // Извлича данните по тип документ и тип производство
            result.AddRange(repo.All<CaseLoadElementGroup>()
                                .Include(x => x.CaseLoadElementTypes)
                                .ThenInclude(x => x.CaseLoadElementTypeRules)
                                .Where(x => (x.IsND == isND) &&
                                            (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                            (x.CaseTypeId == caseCase.CaseTypeId) &&
                                            (x.DocumentTypeId == caseCase.Document.DocumentTypeId) &&
                                            (x.ProcessPriorityId == caseCase.ProcessPriorityId) &&
                                            (x.CaseCodeId == null) &&
                                            ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                .ToList() ?? new List<CaseLoadElementGroup>());

            if (result.Count > 0)
                return result;

            // Извлича данните по шифър
            result.AddRange(repo.All<CaseLoadElementGroup>()
                                .Include(x => x.CaseLoadElementTypes)
                                .ThenInclude(x => x.CaseLoadElementTypeRules)
                                .Where(x => (x.IsND == isND) &&
                                            (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                            (x.CaseTypeId == caseCase.CaseTypeId) &&
                                            (x.DocumentTypeId == null) &&
                                            (x.ProcessPriorityId == null) &&
                                            (x.CaseCodeId == caseCase.CaseCodeId) &&
                                            ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                .ToList() ?? new List<CaseLoadElementGroup>());

            if (result.Count > 0)
                return result;

            // Извлича данните по тип документ
            result.AddRange(repo.All<CaseLoadElementGroup>()
                                .Include(x => x.CaseLoadElementTypes)
                                .ThenInclude(x => x.CaseLoadElementTypeRules)
                                .Where(x => (x.IsND == isND) &&
                                            (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                            (x.CaseTypeId == caseCase.CaseTypeId) &&
                                            (x.DocumentTypeId == caseCase.Document.DocumentTypeId) &&
                                            (x.CaseCodeId == null) &&
                                            (x.ProcessPriorityId == null) &&
                                            ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                .ToList() ?? new List<CaseLoadElementGroup>());

            if (result.Count > 0)
                return result;

            // Извлича данните по основните полета (наказателно дело, инстанция и тип дело)
            result.AddRange(repo.All<CaseLoadElementGroup>()
                                .Include(x => x.CaseLoadElementTypes)
                                .ThenInclude(x => x.CaseLoadElementTypeRules)
                                .Where(x => (x.IsND == isND) &&
                                            (x.CaseInstanceId == caseCase.CaseType.CaseInstanceId) &&
                                            (x.CaseTypeId == caseCase.CaseTypeId) &&
                                            (x.DocumentTypeId == null) &&
                                            (x.CaseCodeId == null) &&
                                            (x.ProcessPriorityId == null) &&
                                            ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                .ToList() ?? new List<CaseLoadElementGroup>());

            return result;
        }

        /// <summary>
        /// Зареждане на комбо в Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadElementGroup(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = ReadAllCaseLoadElementGroupByCase(CaseId)
                                  .Select(x => new SelectListItem()
                                  {
                                      Text = x.Label,
                                      Value = x.Id.ToString()
                                  })
                                  .OrderBy(x => x.Text)
                                  .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Зареждане на комбо с Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="CaseLoadElementGroupeId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadElementType(int CaseLoadElementGroupeId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.All<CaseLoadElementType>()
                                      .Where(x => (x.CaseLoadElementGroupId == CaseLoadElementGroupeId) &&
                                                  ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Зареждане на комбо с Елементи за препокриване
        /// </summary>
        /// <param name="CurrentId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadElementType_Replace(int CurrentId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<CaseLoadElementType>()
                                      .Include(X => X.CaseLoadElementGroup)
                                      .Where(x => (CurrentId > 0 ? x.Id != CurrentId : true) &&
                                                  ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.Label + " (" + x.CaseLoadElementGroup.Label + ")",
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Зареждане на комбо с Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseLoadAddActivity(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var caseCase = repo.AllReadonly<Case>()
                               .Where(x => x.Id == CaseId)
                               .FirstOrDefault();

            var isND = (caseCase.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo);
            var selectListItems = repo.All<CaseLoadAddActivity>()
                                        .Where(x => (x.IsND == isND) &&
                                                    ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.Label,
                                            Value = x.Id.ToString()
                                        })
                                        .OrderBy(x => x.Text)
                                        .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        #endregion

        #region Case Load Element

        /// <summary>
        /// Извличане на данни за Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <returns></returns>
        public IQueryable<CaseLoadElementGroupVM> CaseLoadElementGroup_Select()
        {
            return repo.AllReadonly<CaseLoadElementGroup>()
                .Include(x => x.CaseInstance)
                .Include(x => x.CaseType)
                .Select(x => new CaseLoadElementGroupVM()
                {
                    Id = x.Id,
                    IsNDLabel = (x.IsND) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                    CaseInstanceLabel = (x.CaseInstance != null) ? x.CaseInstance.Label : string.Empty,
                    CaseTypeLabel = (x.CaseType != null) ? x.CaseType.Label : string.Empty,
                    Label = x.Label,
                    DateStart = x.DateStart,
                    DateEnd = x.DateEnd
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на Вид група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadElementGroup_SaveData(CaseLoadElementGroup model)
        {
            try
            {
                model.CaseTypeId = model.CaseTypeId.NumberEmptyToNull();
                model.DocumentTypeId = model.DocumentTypeId.NumberEmptyToNull();
                model.CaseCodeId = model.CaseCodeId.NumberEmptyToNull();
                model.ProcessPriorityId = model.ProcessPriorityId.NumberEmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadElementGroup>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsND = model.IsND;
                    saved.CaseInstanceId = model.CaseInstanceId;
                    saved.CaseTypeId = model.CaseTypeId;
                    saved.CaseCodeId = model.CaseCodeId;
                    saved.DocumentTypeId = model.DocumentTypeId;
                    saved.ProcessPriorityId = model.ProcessPriorityId;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadElementGroup>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Вид група за натовареност по дела Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="CaseLoadElementGroupId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadElementTypeVM> CaseLoadElementType_Select(int CaseLoadElementGroupId)
        {
            return repo.AllReadonly<CaseLoadElementType>()
                       .Where(x => x.CaseLoadElementGroupId == CaseLoadElementGroupId)
                       .Select(x => new CaseLoadElementTypeVM()
                       {
                           Id = x.Id,
                           LoadProcent = x.LoadProcent,
                           Label = x.Label,
                           DateStart = x.DateStart,
                           DateEnd = x.DateEnd
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Елементи към група за натовареност по дела - основни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadElementType_SaveData(CaseLoadElementType model)
        {
            try
            {
                model.ReplaceCaseLoadElementTypeId = model.ReplaceCaseLoadElementTypeId.NumberEmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadElementType>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.LoadProcent = model.LoadProcent;
                    saved.ReplaceCaseLoadElementTypeId = model.ReplaceCaseLoadElementTypeId;
                    saved.CaseLoadElementGroupId = model.CaseLoadElementGroupId;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadElementType>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Вид група за натовареност по дела Id={ model.Id }");
                return false;
            }
        }

        public IQueryable<CaseLoadElementTypeRuleVM> CaseLoadElementTypeRule_Select(int CaseLoadElementTypeId)
        {
            return repo.AllReadonly<CaseLoadElementTypeRule>()
                       .Include(x => x.SessionType)
                       .Include(x => x.SessionResult)
                       .Include(x => x.ActType)
                       .Where(x => x.CaseLoadElementTypeId == CaseLoadElementTypeId && x.DateExpired == null)
                       .Select(x => new CaseLoadElementTypeRuleVM()
                       {
                           Id = x.Id,
                           SessionTypeLabel = x.SessionType.Label,
                           SessionResultLabel = x.SessionResult.Label,
                           ActTypeLabel = x.ActType.Label,
                           IsCreateMotiveText = (x.IsCreateMotive ?? false) ? MessageConstant.Yes : MessageConstant.No,
                           IsCreateCaseText = (x.IsCreateCase ?? false) ? MessageConstant.Yes : MessageConstant.No
                       })
                       .AsQueryable();
        }

        public bool CaseLoadElementTypeRule_SaveData(CaseLoadElementTypeRule model)
        {
            try
            {
                model.SessionTypeId = model.SessionTypeId.NumberEmptyToNull();
                model.SessionResultId = model.SessionResultId.NumberEmptyToNull();
                model.ActTypeId = model.ActTypeId.NumberEmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadElementTypeRule>(model.Id);
                    saved.SessionResultId = model.SessionResultId;
                    saved.SessionTypeId = model.SessionTypeId;
                    saved.ActTypeId = model.ActTypeId;
                    saved.IsCreateMotive = model.IsCreateMotive;
                    saved.IsCreateCase = model.IsCreateCase;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    var caseLoad = repo.GetById<CaseLoadElementType>(model.CaseLoadElementTypeId);
                    model.Label = caseLoad.Label;
                    repo.Add<CaseLoadElementTypeRule>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Вид група за натовареност по дела Id={ model.Id }");
                return false;
            }
        }

        public bool ElementTypeRule_Expired(ExpiredInfoVM model)
        {
            var saved = repo.GetById<CaseLoadElementTypeRule>(model.Id);
            var user = repo.GetById<ApplicationUser>(userContext.UserId);
            var law = repo.GetById<LawUnit>(user.LawUnitId);

            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = null;
                saved.DescriptionExpired = model.DescriptionExpired + " " + law.FullName;
                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Case Load Activity

        /// <summary>
        /// Извличане на данни за Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IQueryable<CaseLoadAddActivityVM> CaseLoadAddActivity_Select()
        {
            return repo.AllReadonly<CaseLoadAddActivity>()
                .Select(x => new CaseLoadAddActivityVM()
                {
                    Id = x.Id,
                    IsNDLabel = (x.IsND) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No,
                    Label = x.Label,
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadAddActivity_SaveData(CaseLoadAddActivity model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadAddActivity>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.IsND = model.IsND;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadAddActivity>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност по дела - допълнителни дейности Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="CaseLoadAddActivityId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadAddActivityIndexVM> CaseLoadAddActivityIndex_Select(int CaseLoadAddActivityId)
        {
            return repo.AllReadonly<CaseLoadAddActivityIndex>()
                       .Include(x => x.CourtType)
                       .Where(x => x.CaseLoadAddActivityId == CaseLoadAddActivityId)
                       .Select(x => new CaseLoadAddActivityIndexVM()
                       {
                           Id = x.Id,
                           CourtTypeLabel = (x.CourtType != null) ? x.CourtType.Label : string.Empty,
                           Label = x.Label,
                           LoadIndex = x.LoadIndex,
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLoadAddActivityIndex_SaveData(CaseLoadAddActivityIndex model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLoadAddActivityIndex>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.CourtTypeId = model.CourtTypeId;
                    saved.LoadIndex = model.LoadIndex;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<CaseLoadAddActivityIndex>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност по дела - допълнителни дейности - стойности по вид съд Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на Натовареност по дела - допълнителни дейности
        /// </summary>
        /// <param name="term"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<LabelValueVM> Get_CaseLoadAddActivity(string term, int? id)
        {
            term = term.SafeLower();
            Expression<Func<CaseLoadAddActivity, bool>> filter = x => x.Label.Contains(term ?? x.Label, StringComparison.InvariantCultureIgnoreCase);
            if (id > 0)
            {
                filter = x => x.Id == id;
            }
            return repo.AllReadonly<CaseLoadAddActivity>()
                            .Where(filter)
                            .Where(x => ((x.DateStart <= DateTime.Now) && ((x.DateEnd ?? DateTime.Now.AddYears(100)) >= DateTime.Now)))
                            .OrderBy(x => x.Label)
                            .Select(x => new LabelValueVM
                            {
                                Value = x.Id.ToString(),
                                Label = x.Label
                            }).ToList();
        }

        #endregion

        #region Judge Load Activity

        /// <summary>
        /// Извличане на данни за Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <returns></returns>
        public IQueryable<JudgeLoadActivityVM> JudgeLoadActivity_Select()
        {
            return repo.AllReadonly<JudgeLoadActivity>()
                       .Select(x => new JudgeLoadActivityVM()
                       {
                           Id = x.Id,
                           Label = x.Label,
                           GroupNo = x.GroupNo,
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Натовареност на съдии - допълнителни дейности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool JudgeLoadActivity_SaveData(JudgeLoadActivity model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<JudgeLoadActivity>(model.Id);
                    saved.Label = model.Label;
                    saved.Description = model.Description;
                    saved.GroupNo = model.GroupNo;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.IsActive = true;
                    repo.Add<JudgeLoadActivity>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност на съдии - допълнителни дейности Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="JudgeLoadActivityId"></param>
        /// <returns></returns>
        public IQueryable<JudgeLoadActivityIndexVM> JudgeLoadActivityIndex_Select(int JudgeLoadActivityId)
        {
            return repo.AllReadonly<JudgeLoadActivityIndex>()
                       .Include(x => x.CourtType)
                       .Where(x => x.JudgeLoadActivityId == JudgeLoadActivityId)
                       .Select(x => new JudgeLoadActivityIndexVM()
                       {
                           Id = x.Id,
                           CourtTypeLabel = (x.CourtType != null) ? x.CourtType.Label : string.Empty,
                           LoadIndex = x.LoadIndex,
                           IsActiveLabel = (x.IsActive) ? NomenclatureConstants.AnswerQuestionTextBG.Yes : NomenclatureConstants.AnswerQuestionTextBG.No
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Натовареност по дела - допълнителни дейности - стойности по вид съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool JudgeLoadActivityIndex_SaveData(JudgeLoadActivityIndex model)
        {
            try
            {
                model.CourtTypeId = model.CourtTypeId.NumberEmptyToNull();
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<JudgeLoadActivityIndex>(model.Id);
                    saved.JudgeLoadActivityId = model.JudgeLoadActivityId;
                    saved.IsActive = model.IsActive;
                    saved.CourtTypeId = model.CourtTypeId;
                    saved.LoadIndex = model.LoadIndex;
                    saved.DateEnd = model.DateEnd.ForceEndDate();
                    saved.DateStart = model.DateStart.ForceStartDate();
                    saved.IsActive = model.IsActive;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    repo.Add<JudgeLoadActivityIndex>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Натовареност по дела - допълнителни дейности - стойности по вид съд Id={ model.Id }");
                return false;
            }
        }

        #endregion

        #region Court Law Unit Activity

        /// <summary>
        /// Извличане на данни за Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="CourtId"></param>
        /// <returns></returns>
        public IQueryable<CourtLawUnitActivityVM> CourtLawUnitActivity_Select(int CourtId)
        {
            return repo.AllReadonly<CourtLawUnitActivity>()
                       .Include(x => x.LawUnit)
                       .Include(x => x.JudgeLoadActivity)
                       .Where(x => x.CourtId == CourtId)
                       .Select(x => new CourtLawUnitActivityVM()
                       {
                           Id = x.Id,
                           LawUnitLabel = x.LawUnit.FullName,
                           ActivityDate = x.ActivityDate,
                           JudgeLoadActivityLabel = x.JudgeLoadActivity.Label,
                           LoadIndex = x.LoadIndex
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на индекс за Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="JudgeLoadActivityId"></param>
        /// <returns></returns>
        private decimal GetLoadIndex_CourtLawUnitActivity(int JudgeLoadActivityId)
        {
            var judgeLoadActivityIndices = repo.AllReadonly<JudgeLoadActivityIndex>()
                                               .Where(x => x.JudgeLoadActivityId == JudgeLoadActivityId)
                                               .ToList();

            var judgeLoadActivityIndice = judgeLoadActivityIndices.Where(x => x.CourtTypeId == userContext.CourtTypeId).FirstOrDefault();

            if (judgeLoadActivityIndice == null)
                judgeLoadActivityIndice = judgeLoadActivityIndices.FirstOrDefault();

            var valueDel = priceService.GetPriceValue(null, NomenclatureConstants.PriceDescKeyWord.KeyJudgeLoadActivity, 0, null, 0, 0, null);

            return (judgeLoadActivityIndice != null) ? judgeLoadActivityIndice.LoadIndex / valueDel : 0;
        }

        /// <summary>
        /// Запис на Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CourtLawUnitActivity_SaveData(CourtLawUnitActivity model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CourtLawUnitActivity>(model.Id);
                    saved.CourtId = model.CourtId;
                    saved.LawUnitId = model.LawUnitId;
                    saved.ActivityDate = model.ActivityDate;

                    if (saved.JudgeLoadActivityId != model.JudgeLoadActivityId)
                    {
                        saved.JudgeLoadActivityId = model.JudgeLoadActivityId;
                        saved.LoadIndex = GetLoadIndex_CourtLawUnitActivity(model.JudgeLoadActivityId);
                    }

                    saved.DateTo = model.DateTo;
                    saved.Description = model.Description;
                    repo.Update(saved);
                    repo.SaveChanges();
                }
                else
                {
                    model.LoadIndex = GetLoadIndex_CourtLawUnitActivity(model.JudgeLoadActivityId);
                    repo.Add<CourtLawUnitActivity>(model);
                    repo.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Допълнителни и административни дейности към съдии по съд Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Проверка дали съществува по дело Допълнителни и административни дейности към съдии по съд
        /// </summary>
        /// <param name="LawUnitId"></param>
        /// <param name="JudgeLoadActivityId"></param>
        /// <param name="ModelId"></param>
        /// <param name="ActivityDate"></param>
        /// <returns></returns>
        public bool IsExistCourtLawUnitActivity(int LawUnitId, int JudgeLoadActivityId, int ModelId, DateTime ActivityDate)
        {
            var judgeLoadActivity = repo.GetById<JudgeLoadActivity>(JudgeLoadActivityId);

            return repo.AllReadonly<CourtLawUnitActivity>()
                       .Include(x => x.JudgeLoadActivity)
                       .Any(x => ((ModelId > 0) ? x.Id != ModelId : true) &&
                                 (x.LawUnitId == LawUnitId) &&
                                 ((judgeLoadActivity.GroupNo == null) ? x.JudgeLoadActivityId == JudgeLoadActivityId : x.JudgeLoadActivity.GroupNo == judgeLoadActivity.GroupNo) &&
                                 (x.ActivityDate.Year == ActivityDate.Year));

        }

        #endregion

        #region Report

        /// <summary>
        /// Натовареност по дела: основни и допълнителни дейности
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="LawUnitId"></param>
        /// <returns></returns>
        public IQueryable<CaseLoadIndexSprVM> CaseLoadIndexSpr_Select(DateTime DateFrom, DateTime DateTo, int? LawUnitId)
        {
            LawUnitId = LawUnitId.NumberEmptyToNull();

            var caseLoadIndices = repo.AllReadonly<CaseLoadIndex>()
                                      .Include(x => x.Case)
                                      .Include(x => x.LawUnit)
                                      .Where(x => x.CourtId == userContext.CourtId &&
                                                  x.DateActivity >= DateFrom && x.DateActivity <= DateTo &&
                                                  ((LawUnitId != null) ? x.LawUnitId == LawUnitId : true))
                                      .ToList();

            List<CaseLoadIndexSprVM> _result = new List<CaseLoadIndexSprVM>();

            foreach (var caseLoad in caseLoadIndices)
            {
                var caseLoadIndexSpr = _result.Where(x => x.CaseId == caseLoad.CaseId && x.LawUnitId == caseLoad.LawUnitId).FirstOrDefault();

                if (caseLoadIndexSpr == null)
                {
                    CaseLoadIndexSprVM caseLoadIndex = new CaseLoadIndexSprVM()
                    {
                        CaseId = caseLoad.CaseId,
                        CaseName = caseLoad.Case.RegNumber + "/" + caseLoad.Case.RegDate.ToString("dd.MM.yyyy"),
                        LawUnitId = caseLoad.LawUnitId,
                        LawUnitName = caseLoad.LawUnit.FullName,
                        CalcValue = (caseLoad.IsMainActivity) ? Math.Round(caseLoad.BaseIndex * (caseLoad.LoadProcent / 100), 2) : caseLoad.LoadIndex
                    };

                    _result.Add(caseLoadIndex);
                }
                else
                {
                    caseLoadIndexSpr.CalcValue += (caseLoad.IsMainActivity) ? Math.Round(caseLoad.BaseIndex * (caseLoad.LoadProcent / 100), 2) : caseLoad.LoadIndex;
                }
            }

            return _result.AsQueryable();
        }

        /// <summary>
        /// Натоварване на съдии извън дело
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="LawUnitId"></param>
        /// <returns></returns>
        public IQueryable<LawUnitLoadSprVM> CourtLawUnitActivitySpr_Select(DateTime DateFrom, DateTime DateTo, int? LawUnitId)
        {
            LawUnitId = LawUnitId.NumberEmptyToNull();
            var courtLawUnitActivities = repo.AllReadonly<CourtLawUnitActivity>()
                                             .Include(x => x.LawUnit)
                                             .Where(x => x.CourtId == userContext.CourtId &&
                                                         x.ActivityDate >= DateFrom && x.ActivityDate <= DateTo &&
                                                         ((LawUnitId != null) ? x.LawUnitId == LawUnitId : true))
                                             .ToList();

            List<LawUnitLoadSprVM> _result = new List<LawUnitLoadSprVM>();

            foreach (var courtLawUnitActivity in courtLawUnitActivities)
            {
                var lawUnitLoadSprVM = _result.Where(x => x.LawUnitId == courtLawUnitActivity.LawUnitId).FirstOrDefault();

                if (lawUnitLoadSprVM == null)
                {
                    LawUnitLoadSprVM unitLoadSprVM = new LawUnitLoadSprVM()
                    {
                        LawUnitId = courtLawUnitActivity.LawUnitId,
                        LawUnitLabel = courtLawUnitActivity.LawUnit.FullName,
                        LoadIndex = courtLawUnitActivity.LoadIndex,
                        SumLoadIndex = courtLawUnitActivity.LoadIndex
                    };

                    _result.Add(unitLoadSprVM);
                }
                else
                {
                    lawUnitLoadSprVM.LoadIndex += courtLawUnitActivity.LoadIndex;
                    lawUnitLoadSprVM.SumLoadIndex += courtLawUnitActivity.LoadIndex;
                }
            }

            return _result.AsQueryable();
        }

        /// <summary>
        /// Натовареност - извън и в дело
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="LawUnitId"></param>
        /// <returns></returns>
        public IQueryable<LawUnitLoadSprVM> LawUnitActivitySpr_Select(DateTime DateFrom, DateTime DateTo, int? LawUnitId)
        {
            LawUnitId = LawUnitId.NumberEmptyToNull();
            var caseLoadIndexSprs = CaseLoadIndexSpr_Select(DateFrom, DateTo, LawUnitId).ToList();
            var lawUnitLoadSprs = CourtLawUnitActivitySpr_Select(DateFrom, DateTo, LawUnitId).ToList();

            foreach (var caseLoad in caseLoadIndexSprs)
            {
                var unitLoadSprVMs = lawUnitLoadSprs.Where(x => x.LawUnitId == caseLoad.LawUnitId).FirstOrDefault();

                if (unitLoadSprVMs == null)
                {
                    LawUnitLoadSprVM unitLoadSprVM = new LawUnitLoadSprVM()
                    {
                        LawUnitId = caseLoad.LawUnitId,
                        LawUnitLabel = caseLoad.LawUnitName,
                        CaseLoadIndex = caseLoad.CalcValue,
                        SumLoadIndex = caseLoad.CalcValue
                    };

                    lawUnitLoadSprs.Add(unitLoadSprVM);
                }
                else
                {
                    unitLoadSprVMs.CaseLoadIndex += caseLoad.CalcValue;
                    unitLoadSprVMs.SumLoadIndex = unitLoadSprVMs.CaseLoadIndex + unitLoadSprVMs.LoadIndex;
                }
            }

            return lawUnitLoadSprs.AsQueryable();
        }

        #endregion
    }
}
