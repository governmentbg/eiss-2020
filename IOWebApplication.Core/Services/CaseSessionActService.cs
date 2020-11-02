// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionActService : BaseService, ICaseSessionActService
    {
        private readonly ICounterService counterService;
        private readonly ICaseSessionActCoordinationService coordinationService;
        private readonly IWorkTaskService taskService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly ICasePersonService casePersonService;
        private readonly ICasePersonLinkService casePersonLinkService;
        private readonly ICaseFastProcessService caseFastProcessService;
        private readonly IMQEpepService mqEpepService;
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly ICaseLifecycleService caseLifecycleService;
        private readonly ICaseLoadIndexService caseLoadIndexService;

        public CaseSessionActService(
            ILogger<CaseSessionActService> _logger,
            IRepository _repo,
            AutoMapper.IMapper _mapper,
            IUserContext _userContext,
            IWorkTaskService _taskService,
            ICaseSessionActCoordinationService _coordinationService,
            ICaseLawUnitService _caseLawUnitService,
            ICasePersonService _casePersonService,
            ICounterService _counterService,
            ICaseFastProcessService _caseFastProcessService,
            IMQEpepService _mqEpepService,
            ICaseDeadlineService _caseDeadlineService,
            ICaseLifecycleService _caseLifecycleService,
            ICaseLoadIndexService _caseLoadIndexService,
            ICasePersonLinkService _casePersonLinkService)
        {
            logger = _logger;
            repo = _repo;
            mapper = _mapper;
            userContext = _userContext;
            taskService = _taskService;
            counterService = _counterService;
            coordinationService = _coordinationService;
            caseLawUnitService = _caseLawUnitService;
            casePersonService = _casePersonService;
            caseFastProcessService = _caseFastProcessService;
            mqEpepService = _mqEpepService;
            caseDeadlineService = _caseDeadlineService;
            caseLifecycleService = _caseLifecycleService;
            caseLoadIndexService = _caseLoadIndexService;
            casePersonLinkService = _casePersonLinkService;
        }

        /// <summary>
        /// Извличане на данни за съдебни актове
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="caseId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="showExpired"></param>
        /// <param name="year"></param>
        /// <param name="caseRegNumber"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActVM> CaseSessionAct_Select(int caseSessionId, int? caseId, DateTime? DateFrom,
             DateTime? DateTo, int? year, string caseRegNumber, bool showExpired = false)
        {

            Expression<Func<CaseSessionAct, bool>> yearSearch = x => true;
            if ((year ?? 0) > 0)
                yearSearch = x => x.Case.RegDate.Year == year;

            Expression<Func<CaseSessionAct, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(caseRegNumber))
                caseRegnumberSearch = x => x.Case.RegNumber.EndsWith(caseRegNumber.ToShortCaseNumber(), StringComparison.InvariantCultureIgnoreCase);

            return repo.AllReadonly<CaseSessionAct>()
                .Include(x => x.CaseSession)
                .ThenInclude(x => x.Case)
                .Include(x => x.CaseSession)
                .ThenInclude(x => x.SessionType)
                .Include(x => x.ActType)
                .Include(x => x.ActState)
                .Where(this.FilterExpireInfo<CaseSessionAct>(showExpired))
                .Where(x => ((caseSessionId > 0) ? (x.CaseSessionId == caseSessionId) : true) &&
                            ((x.RegDate == null) ? true : ((DateFrom != null) ? ((x.RegDate.Value.Date >= (DateFrom ?? DateTime.Now).Date) && (x.RegDate.Value.Date <= (DateTo ?? DateTime.Now).Date)) : true)) &&
                            (((caseId ?? 0) > 0) ? (x.CaseSession.CaseId == caseId) : true) &&
                            ((caseSessionId < 1 && (caseId ?? 0) < 1) ? x.CaseSession.Case.CourtId == userContext.CourtId : true))
                .Where(yearSearch)
                .Where(caseRegnumberSearch)
                .Select(x => new CaseSessionActVM()
                {
                    Id = x.Id,
                    CaseSessionId = x.CaseSessionId,
                    CaseId = x.CaseSession.CaseId,
                    CaseSessionLabel = (x.CaseSession != null) ? x.CaseSession.SessionType.Label + "/" + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm") : string.Empty,
                    CaseLabel = x.CaseSession.Case.RegNumber + "/" + x.CaseSession.Case.RegDate.ToString("dd.MM.yyyy"),
                    ActTypeLabel = (x.ActType != null) ? x.ActType.Label : string.Empty,
                    //ActResultLabel = (x.ActResult != null) ? x.ActResult.Label : string.Empty,
                    ActStateLabel = (x.ActState != null) ? x.ActState.Label : string.Empty,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate.Value,
                    IsFinalDoc = x.IsFinalDoc,
                    DateWrt = x.DateWrt,
                    EcliCode = x.EcliCode,
                    Description = x.Description
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на съдебни актове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionAct_SaveData(CaseSessionAct model)
        {
            try
            {
                bool isDifFinishDoc = false;
                model.SecretaryUserId = model.SecretaryUserId.EmptyToNull().EmptyToNull("-1");
                model.ActKindId = model.ActKindId.EmptyToNull();
                model.ActResultId = model.ActResultId.EmptyToNull();
                model.ActComplainResultId = model.ActComplainResultId.NumberEmptyToNull();
                model.ActComplainIndexId = model.ActComplainIndexId.NumberEmptyToNull();
                model.ActISPNReasonId = model.ActISPNReasonId.NumberEmptyToNull();
                model.ActISPNDebtorStateId = model.ActISPNDebtorStateId.NumberEmptyToNull();
                model.RelatedActId = model.RelatedActId.NumberEmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionAct>(model.Id);
                    isDifFinishDoc = saved.IsFinalDoc != model.IsFinalDoc;

                    saved.CaseId = model.CaseId;
                    saved.CourtId = model.CourtId;
                    saved.CaseSessionId = model.CaseSessionId;
                    saved.ActTypeId = model.ActTypeId;
                    saved.ActKindId = model.ActKindId;
                    saved.RelatedActId = model.RelatedActId;
                    saved.ActResultId = model.ActResultId;
                    saved.ActStateId = model.ActStateId;
                    saved.IsFinalDoc = model.IsFinalDoc;
                    saved.IsReadyForPublish = model.IsReadyForPublish;
                    saved.CanAppeal = model.CanAppeal;
                    saved.ActInforcedDate = model.ActInforcedDate;
                    saved.ActMotivesDeclaredDate = model.ActMotivesDeclaredDate;
                    saved.SecretaryUserId = model.SecretaryUserId;
                    saved.ActComplainResultId = (model.IsFinalDoc) ? model.ActComplainResultId : null;
                    saved.ActComplainIndexId = model.ActComplainIndexId;
                    saved.ActISPNReasonId = model.ActISPNReasonId;
                    saved.ActISPNDebtorStateId = model.ActISPNDebtorStateId;
                    saved.ActTerm = model.ActTerm;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    CreateHistory<CaseSessionAct, CaseSessionActH>(saved);

                    caseDeadlineService.DeadLineMotive(saved);

                    repo.Update(saved);
                    repo.SaveChanges();

                    if (isDifFinishDoc && saved.ActDeclaredDate != null)
                    {
                        if (model.IsFinalDoc)
                            caseLifecycleService.CaseLifecycle_CloseInterval(model.CaseId ?? 0, model.Id, model.ActDeclaredDate ?? DateTime.Now);
                        else
                            caseLifecycleService.CaseLifecycle_UndoCloseInterval(model.CaseId ?? 0, model.Id);
                    }

                    mqEpepService.AppendCaseSessionAct(saved, EpepConstants.ServiceMethod.Add);
                }
                else
                {
                    //Insert
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    CreateHistory<CaseSessionAct, CaseSessionActH>(model);
                    repo.Add<CaseSessionAct>(model);
                    repo.SaveChanges();

                    caseDeadlineService.DeadLineMotive(model);
                    repo.SaveChanges();
                }

                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(model.CaseSessionId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на акт Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Запис на диспозитив
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dispositiv"></param>
        /// <returns></returns>
        public bool CaseSessionAct_SaveDispositiv(int id, string dispositiv)
        {
            try
            {
                var saved = repo.GetById<CaseSessionAct>(id);

                saved.Description = dispositiv;
                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;
                //Ако акта няма създател и акта все още не е издаден 
                if (string.IsNullOrEmpty(saved.ActCreatorUserId) && string.IsNullOrEmpty(saved.RegNumber))
                {
                    saved.ActCreatorUserId = userContext.UserId;
                }
                CreateHistory<CaseSessionAct, CaseSessionActH>(saved);

                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CaseSessionAct_SaveDispositiv;ID={id}");
                return false;
            }
        }

        public bool CaseSessionAct_SaveMotiveCreator(int id)
        {
            try
            {
                var saved = repo.GetById<CaseSessionAct>(id);

                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;
                //Ако акта няма създател и акта все още не е издаден 
                if (string.IsNullOrEmpty(saved.MotiveCreatorUserId))
                {
                    saved.MotiveCreatorUserId = userContext.UserId;
                }
                CreateHistory<CaseSessionAct, CaseSessionActH>(saved);

                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CaseSessionAct_SaveDispositiv;ID={id}");
                return false;
            }
        }

        /// <summary>
        /// Проверява правата на достъп до бланката на неизготвен акт
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public (bool canAccess, string lawunitName) CheckActBlankAccess(int id)
        {
            var model = repo.GetById<CaseSessionAct>(id);
            //Ако акта е постановен - има достъп
            if (model.ActDeclaredDate != null)
            {
                return (true, string.Empty);
            }
            return checkBlankAccess(model, model.ActCreatorUserId);
        }

        /// <summary>
        /// Проверява правата на достъп до бланката на неизготвени мотиви
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public (bool canAccess, string lawunitName) CheckMotiveBlankAccess(int id)
        {
            var model = repo.GetById<CaseSessionAct>(id);
            //Ако мотивите имат дата на обявяване - има достъп
            if (model.ActMotivesDeclaredDate.HasValue)
            {
                return (true, string.Empty);
            }
            return checkBlankAccess(model, model.MotiveCreatorUserId);
        }

        private (bool canAccess, string lawunitName) checkBlankAccess(CaseSessionAct model, string userCreatorId)
        {
            (bool canAccess, string lawunitName) result = (false, string.Empty);
            //Достъп имат изготвилия бланката или всеки от делото, ако все още не е въведен текст
            if (string.IsNullOrEmpty(userCreatorId) || userCreatorId == userContext.UserId)
            {
                result.canAccess = true;
                return result;
            }

            var sessionReporterLawUnitId = repo.AllReadonly<CaseLawUnit>()
                                               .Include(x => x.CaseSession)
                                               .Where(x => x.CaseId == model.CaseId && x.CaseSessionId == model.CaseSessionId)
                                               .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                               .Select(x => x.LawUnitId)
                                               .FirstOrDefault();

            result.canAccess = sessionReporterLawUnitId == userContext.LawUnitId;

            if (!result.canAccess)
            {
                result.lawunitName = repo.All<ApplicationUser>()
                                                .Include(x => x.LawUnit)
                                                .Where(x => x.Id == userCreatorId)
                                                .Select(x => (x.LawUnit != null) ? x.LawUnit.FullName : "")
                                                .FirstOrDefault();
            }
            return result;
        }

        private (bool canAccess, string lawunitName) checkActFileAccess(CaseSessionAct model, string userCreatorId)
        {
            (bool canAccess, string lawunitName) result = (false, string.Empty);
            //Достъп имат изготвилия бланката или всеки от делото, ако все още не е въведен текст
            if (string.IsNullOrEmpty(userCreatorId) || userCreatorId == userContext.UserId)
            {
                result.canAccess = true;
                return result;
            }

            var sessionReporterLawUnitId = repo.AllReadonly<CaseLawUnit>()
                                               .Include(x => x.CaseSession)
                                               .Where(x => x.CaseId == model.CaseId && x.CaseSessionId == model.CaseSessionId)
                                               .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                               .Select(x => x.LawUnitId)
                                               .FirstOrDefault();

            result.canAccess = sessionReporterLawUnitId == userContext.LawUnitId;

            if (!result.canAccess)
            {
                if (repo.AllReadonly<WorkTask>()
                            .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct && x.SourceId == model.Id)
                            .Where(x => x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_Sign)
                            .Where(x => x.TaskStateId != WorkTaskConstants.States.Deleted)
                            .Where(x => x.UserId == userContext.UserId)
                            .Any())
                {
                    result.canAccess = true;
                    return result;
                }

                if (!result.canAccess)
                {
                    result.lawunitName = repo.All<ApplicationUser>()
                                                    .Include(x => x.LawUnit)
                                                    .Where(x => x.Id == userCreatorId)
                                                    .Select(x => (x.LawUnit != null) ? x.LawUnit.FullName : "")
                                                    .FirstOrDefault();
                }
            }
            return result;
        }


        public SaveResultVM CaseSessionAct_RegisterAct(int id)
        {
            var model = repo.GetById<CaseSessionAct>(id);
            return CaseSessionAct_RegisterAct(model);
        }

        /// <summary>
        /// Регистриране на съдебни актове
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SaveResultVM CaseSessionAct_RegisterAct(CaseSessionAct model)
        {
            if (!string.IsNullOrEmpty(model.RegNumber))
            {
                return new SaveResultVM(true);
            }
            var _case = repo.AllReadonly<CaseSession>()
                                            .Include(x => x.Case)
                                            .Where(x => x.Id == model.CaseSessionId)
                                            .Select(x => x.Case)
                                            .FirstOrDefault();

            if (counterService.Counter_GetActCounter(model, _case.CaseGroupId, _case.CourtId))
            {
                GenerateActEcliNumber(model);
                model.ActStateId = NomenclatureConstants.SessionActState.Registered;
                CreateHistory<CaseSessionAct, CaseSessionActH>(model);
                repo.Update(model);
                repo.SaveChanges();
                return new SaveResultVM(true, null, "register");
            }
            return new SaveResultVM(false, "Проблем при регистриране на акт");
        }

        /// <summary>
        /// ИЗпращане за съгласуване
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool SendForCoordination_Init(int caseSessionActId, long taskId)
        {
            var act = repo.GetById<CaseSessionAct>(caseSessionActId);
            var model = getCaseLawUnitByAct(caseSessionActId);

            bool hasCoordinationForAct = repo.AllReadonly<CaseSessionActCoordination>()
                                                    .Any(x => x.CaseSessionActId == caseSessionActId);
            if (hasCoordinationForAct)
            {
                var prevCoordicationTasks = repo.All<WorkTask>()
                                                    .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct && x.SourceId == caseSessionActId)
                                                    .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                    .ToList();
                foreach (var item in prevCoordicationTasks)
                {
                    item.TaskStateId = WorkTaskConstants.States.Deleted;
                    repo.SaveChanges();
                }
            }
            foreach (var caseLawUnit in model)
            {
                //Ако вече има създадени записи за особени мнения към акта - не се създават нови
                if (!hasCoordinationForAct)
                {
                    var coordination = new CaseSessionActCoordination()
                    {
                        CaseId = act.CaseId,
                        CourtId = act.CourtId,
                        CaseSessionActId = act.Id,
                        CaseLawUnitId = caseLawUnit.Id,
                        ActCoordinationTypeId = NomenclatureConstants.ActCoordinationTypes.New,
                        UserId = userContext.UserId,
                        DateWrt = DateTime.Now

                    };
                    repo.Add(coordination);
                    repo.SaveChanges();
                }

                var userId = GetUserIdByLawUnitId(caseLawUnit.LawUnitId);
                if (!string.IsNullOrEmpty(userId))
                {
                    var newTask = new WorkTaskEditVM()
                    {
                        ParentTaskId = taskId,
                        SourceType = SourceTypeSelectVM.CaseSessionAct,
                        SourceId = caseSessionActId,
                        TaskTypeId = WorkTaskConstants.Types.CaseSessionAct_Coordinate,
                        TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                        UserId = userId,
                    };
                    taskService.CreateTask(newTask);
                }
            }

            return model.Count > 0;
        }

        /// <summary>
        /// Извличане на състав от акт
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <returns></returns>
        private ICollection<CaseLawUnit> getCaseLawUnitByAct(int caseSessionActId)
        {
            var actInfo = repo.AllReadonly<CaseSessionAct>().Where(x => x.Id == caseSessionActId)
                                .Select(x => new { RegDate = (x.RegDate ?? DateTime.Now), x.CaseSessionId, x.CaseId }).FirstOrDefault();
            return repo.AllReadonly<CaseLawUnit>()
                            .Where(x => x.CaseId == actInfo.CaseId && x.CaseSessionId == actInfo.CaseSessionId)
                            .Where(x => NomenclatureConstants.JudgeRole.JudgeRolesListMain.Contains(x.JudgeRoleId))
                            .Where(x => x.DateFrom.Date <= actInfo.RegDate.Date && (x.DateTo ?? DateTime.MaxValue) > actInfo.RegDate)
                            .OrderByDescending(x => x.JudgeRoleId)
                            .ToList();
        }

        /// <summary>
        /// Пращане за подписване
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public SaveResultVM SendForSign_Init(int caseSessionActId, long taskId)
        {
            SaveResultVM result = new SaveResultVM();
            var act = repo.AllReadonly<CaseSessionAct>()
                                .Include(x => x.CaseSession)
                                .ThenInclude(x => x.SessionType)
                                .Where(x => x.Id == caseSessionActId)
                                .FirstOrDefault();

            Expression<Func<CaseLawUnit, bool>> judgeFilter = x => true;
            if (act.ActTypeId == NomenclatureConstants.ActType.Protokol && act.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
            {
                //Когато акта е от вид протокол в ОСЗ се подписва само от председателя на състава
                judgeFilter = x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel;
            }
            var model = getCaseLawUnitByAct(caseSessionActId).AsQueryable()
                            .Where(judgeFilter)
                            .OrderByDescending(x => x.JudgeRoleId)
                            .ToList();

            foreach (var caseLawUnit in model)
            {

                var userId = GetUserIdByLawUnitId(caseLawUnit.LawUnitId);
                if (!string.IsNullOrEmpty(userId))
                {
                    var newTask = new WorkTaskEditVM()
                    {
                        ParentTaskId = taskId,
                        SourceType = SourceTypeSelectVM.CaseSessionAct,
                        SourceId = caseSessionActId,
                        TaskTypeId = WorkTaskConstants.Types.CaseSessionAct_Sign,
                        TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                        UserId = userId,
                    };
                    taskService.CreateTask(newTask);
                }
            }
            if (act.RegDate != null)
            {
                var coordinations = coordinationService.CaseSessionActCoordination_Select(caseSessionActId).Where(x => (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.AcceptWithOpinion) || (x.ActCoordinationTypeId == NomenclatureConstants.ActCoordinationTypes.DontAccept)).ToList();
                foreach (var coordination in coordinations)
                {

                    var userId = GetUserIdByLawUnitId(coordination.LawUnitId);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var newTask = new WorkTaskEditVM()
                        {
                            ParentTaskId = taskId,
                            SourceType = SourceTypeSelectVM.CaseSessionAct,
                            SourceId = caseSessionActId,
                            SubSourceId = coordination.Id,
                            TaskTypeId = WorkTaskConstants.Types.CaseSessionActCoordination_Sign,
                            TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                            UserId = userId,
                        };
                        taskService.CreateTask(newTask);
                    }
                }
            }

            result.Result = model.Count > 0;
            if (!result.Result)
            {
                result.ErrorMessage = "Няма съдебен състав по заседанието";
            }

            return result;
        }

        /// <summary>
        /// Извличане на данни за съдебни актове
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CaseSessionAct CaseSessionAct_GetFullInfo(int id)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Include(x => x.ActType)
                             .Include(x => x.CaseSession)
                             .ThenInclude(x => x.SessionType)
                             .Where(x => x.Id == id)
                             .FirstOrDefault();

            var _case = repo.AllReadonly<Case>()
                             .Include(x => x.Court)
                             .Include(x => x.CaseType)
                             .Include(x => x.CasePersons)
                             .ThenInclude(x => x.PersonRole)
                             .Include(x => x.CasePersons)
                             .ThenInclude(x => x.Addresses)
                             .ThenInclude(x => x.Address)
                             .Include(x => x.CaseLawUnits)
                             .ThenInclude(x => x.LawUnit)
                             .Include(x => x.CaseLawUnits)
                             .ThenInclude(x => x.JudgeRole)
                             .Where(x => x.Id == result.CaseSession.CaseId)
                             .FirstOrDefault();

            result.CaseSession.Case = _case;
            return result;

            //return repo.AllReadonly<CaseSessionAct>()
            //                 .Include(x => x.ActType)
            //                 .Include(x => x.CaseSession)
            //                 .ThenInclude(x => x.SessionType)

            //                 .Include(x => x.CaseSession)
            //                 .ThenInclude(x => x.Case)
            //                 .ThenInclude(x => x.CaseType)

            //                 .Include(x => x.CaseSession)
            //                 .ThenInclude(x => x.Case)
            //                 .ThenInclude(x => x.CasePersons)

            //                 .Include(x => x.CaseSession)
            //                 .ThenInclude(x => x.Case)
            //                 .ThenInclude(x => x.CaseLawUnits)
            //                 .ThenInclude(x => x.JudgeRole)

            //                 .Include(x => x.CaseSession)
            //                 .ThenInclude(x => x.Case)
            //                 .ThenInclude(x => x.CaseLawUnits)
            //                 .ThenInclude(x => x.LawUnit)

            //                 .Include(x => x.CaseSession)
            //                 .ThenInclude(x => x.Case)
            //                 .ThenInclude(x => x.Court)
            //                 .Where(x => x.Id == id)
            //                 .FirstOrDefault();
        }

        /// <summary>
        /// Изпращане за подписване на мотив
        /// </summary>
        /// <param name="caseSessionActId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public bool SendForSignMotives_Init(int caseSessionActId, long taskId)
        {
            var act = repo.GetById<CaseSessionAct>(caseSessionActId);
            var model = getCaseLawUnitByAct(caseSessionActId);

            foreach (var caseLawUnit in model)
            {

                var userId = GetUserIdByLawUnitId(caseLawUnit.LawUnitId);
                if (!string.IsNullOrEmpty(userId))
                {
                    var newTask = new WorkTaskEditVM()
                    {
                        ParentTaskId = taskId,
                        SourceType = SourceTypeSelectVM.CaseSessionAct,
                        SourceId = caseSessionActId,
                        TaskTypeId = WorkTaskConstants.Types.CaseSessionActMotives_Sign,
                        TaskExecutionId = WorkTaskConstants.TaskExecution.ByUser,
                        UserId = userId,
                    };
                    taskService.CreateTask(newTask);
                }
            }


            return model.Count > 0;
        }

        /// <summary>
        /// Зареждане в комбо на актовете от заседания по ID на дело
        /// </summary>
        /// <param name="caseId">ID на акт</param>
        /// <param name="IsFinal">Финален документ. Ако е null не се взема предвид</param>
        /// <param name="IsDecreed">Постановен. Ако е null не се взема предвид</param>
        /// <param name="IsReadyForPublish">Готов за публикуване. Ако е null не се взема предвид</param>
        /// <param name="IsActInforced">Влязъл в сила. Ако е null не се взема предвид</param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList(int caseId, bool? IsFinal = null, bool? IsDecreed = null, bool? IsReadyForPublish = null, bool? IsActInforced = null, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.All<CaseSessionAct>()
                             .Include(x => x.CaseSession)
                             .Include(x => x.ActType)
                             .Include(x => x.ActState)
                             .Where(x => (x.CaseSession.CaseId == caseId) &&
                                         (IsFinal != null ? x.IsFinalDoc : true) &&
                                         (IsDecreed != null ? x.ActDate != null : true) &&
                                         (IsReadyForPublish != null ? x.IsReadyForPublish : true) &&
                                         (IsActInforced != null ? x.ActInforcedDate != null : true))
                .Select(x => new SelectListItem()
                {
                    Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Извличане на съдебни актове по сесия за комбобокс
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownListBySessionId(int caseSessionId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Include(x => x.CaseSession)
                             .Include(x => x.ActType)
                             .Include(x => x.ActState)
                             .Where(x => x.CaseSessionId == caseSessionId)
                .Select(x => new SelectListItem()
                {
                    Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Извличане на данни за съдебни актове за страните (лява/дясна)
        /// </summary>
        /// <param name="models"></param>
        /// <param name="caseSessionActPrint"></param>
        private void FillLeftRightSide_CasePersons(List<CasePersonListVM> models, CaseSessionActPrintVM caseSessionActPrint)
        {
            string result = string.Empty;

            caseSessionActPrint.LeftSide = new List<string>();
            var LeftSideName = new List<string>();
            foreach (var casePerson in models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide))
            {
                var person = casePerson.PersonRoleLabel + "а " + casePerson.FullName + " " + casePerson.UicTypeLabel + ": " + casePerson.Uic + " " + (!string.IsNullOrEmpty(casePerson.AddressString) ? " " + casePerson.AddressString : string.Empty);
                var linkListVM = casePersonLinkService.GetLinkForPerson(casePerson.Id, false, 0, null);
                if (linkListVM != null)
                {
                    person = person + " " + string.Join(", ", linkListVM.Select(x => x.LabelWithoutFirstPerson));
                }
                caseSessionActPrint.LeftSide.Add(person);
                var personName = casePerson.FullName + " " + casePerson.UicTypeLabel + ": " + casePerson.Uic;
                LeftSideName.Add(personName);

            }
            caseSessionActPrint.LeftSideName = string.Join(" и ", LeftSideName.Select(x => x));
            caseSessionActPrint.LeftSideCurrentAddress = string.Join(", ", models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide && !string.IsNullOrEmpty(x.CurrentAddressString)).Select(x => x.CurrentAddressString));
            caseSessionActPrint.LeftSideWorkAddress = string.Join(", ", models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide && !string.IsNullOrEmpty(x.WorkAddressString)).Select(x => x.WorkAddressString));

            caseSessionActPrint.RightSide = new List<string>();
            var RightSideName = new List<string>();
            foreach (var casePerson in models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide))
            {
                var person = casePerson.PersonRoleLabel + "ът " + casePerson.FullName + " " + casePerson.UicTypeLabel + ": " + casePerson.Uic + " " + (!string.IsNullOrEmpty(casePerson.AddressString) ? " " + casePerson.AddressString : string.Empty);
                caseSessionActPrint.RightSide.Add(person);
                var personName = casePerson.FullName + " " + casePerson.UicTypeLabel + ": " + casePerson.Uic;
                RightSideName.Add(personName);
            }
            caseSessionActPrint.RightSideName = string.Join(" и ", RightSideName.Select(x => x));
            caseSessionActPrint.RightSideCurrentAddress = string.Join(", ", models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide && !string.IsNullOrEmpty(x.CurrentAddressString)).Select(x => x.CurrentAddressString));
            caseSessionActPrint.RightSideWorkAddress = string.Join(", ", models.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide && !string.IsNullOrEmpty(x.WorkAddressString)).Select(x => x.WorkAddressString));
        }

        /// <summary>
        /// Принтиране на съдебни актове
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CaseSessionActPrintVM CaseSessionAct_GetForPrint(int id)
        {
            CaseSessionActPrintVM result = new CaseSessionActPrintVM();

            var act = repo.AllReadonly<CaseSessionAct>()
                                     .Include(x => x.CaseSession)
                                     .ThenInclude(x => x.SessionType)
                                     .Include(x => x.CaseSession)
                                     .ThenInclude(x => x.Case)
                                     .ThenInclude(x => x.Court)
                                     .ThenInclude(x => x.CourtRegion)
                                     .Include(x => x.CaseSession)
                                     .ThenInclude(x => x.Case)
                                     .ThenInclude(x => x.CaseType)
                                     .Include(x => x.ActType)
                                     .Include(x => x.ActKind)
                                     .Include(x => x.RelatedAct)
                                     .ThenInclude(x => x.ActType)
                                     .FirstOrDefault(x => x.Id == id);

            if (act.ActTypeId == NomenclatureConstants.ActType.Protokol && act.CaseSession.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession)
            {
                result.ChairmanSignOnly = true;
            }

            DateTime actDate = act.RegDate ?? DateTime.Now;

            result.Id = id;
            result.ActFormatType = act.ActType.ActFormatType;
            result.ActTypeCode = act.ActType.Code;
            result.ActTypeName = act.ActType.Label;
            result.BlankHeaderText = act.ActType.BlankHeaderText;
            result.BlankActTypeName = act.ActType.BlankLabel;
            result.ActKindBlankName = (act.ActKind != null) ? act.ActKind.BlankName : "";
            result.ActKindDescription = (act.ActKind != null) ? act.ActKind.Description : "";
            result.ActRegNumber = act.RegNumber;
            result.Dispositiv = act.Description;
            result.ActDeclaredDate = act.ActDeclaredDate;
            result.ActRegDate = (act.RegDate != null) ? act.RegDate.Value.ToString("dd.MM.yyyy") : "";
            result.ActRegYear = (act.RegDate != null) ? act.RegDate.Value.Year.ToString() : "";
            result.BlankDecisionText = act.ActType.BlankDecisionText;
            result.CourtCity = act.CaseSession.Case.Court.CityName;
            result.CourtName = act.CaseSession.Case.Court.Label;
            result.CourtLogo = act.CaseSession.Case.Court.CourtLogo;
            result.RelatedActTypeName = (act.RelatedAct != null) ? act.RelatedAct.ActType.Label : string.Empty;
            result.RelatedActNumber = (act.RelatedAct != null) ? act.RelatedAct.RegNumber : string.Empty;
            result.RelatedActDate = (act.RelatedAct != null) ? (act.RelatedAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty;
            result.RelatedActYear = (act.RelatedAct != null) ? (act.RelatedAct.RegDate ?? DateTime.Now).Year.ToString() : string.Empty;
            result.RelatedActDispositive = (act.RelatedAct != null) ? act.RelatedAct.Description : string.Empty;

            var courtRegion = act.CaseSession.Case.Court.CourtRegion;
            result.CourtParent = string.Empty;
            if (courtRegion != null)
            {
                if (courtRegion.ParentId > 0)
                {
                    result.CourtParent = repo.GetById<CourtRegion>(courtRegion.ParentId).Label;
                }
            }


            //result.CourtParent = (act.CaseSession.Case.Court.CourtRegion != null) ? repo.GetById<CourtRegion>(act.CaseSession.Case.Court.CourtRegion.ParentId).Label : string.Empty;
            result.SessionTypeName = act.CaseSession.SessionType.Label;
            result.SessionActLabel = act.CaseSession.SessionType.SessionActLabel;
            result.SessionStateId = act.CaseSession.SessionStateId;
            result.SessionDate = act.CaseSession.DateFrom;

            var firstSessionMeeting = repo.AllReadonly<CaseSessionMeeting>()
                                        .Where(x => x.CaseSessionId == act.CaseSessionId)
                                        .OrderBy(x => x.DateFrom)
                                        .FirstOrDefault();

            //Началния час на заседанието се взема от първата сесия на заседанието, 15.07.2020 КБорисов
            if (firstSessionMeeting != null)
            {
                result.SessionDate = firstSessionMeeting.DateFrom;
            }

            result.CaseId = act.CaseSession.CaseId;
            result.CaseTypeName = act.CaseSession.Case.CaseType.Label;
            result.CaseRegShortNumber = act.CaseSession.Case.ShortNumber;
            result.CaseRegNumber = act.CaseSession.Case.RegNumber;
            result.CaseRegYear = act.CaseSession.Case.RegDate.Year;
            result.ActTerm = act.ActTerm;
            result.AnswerActRegNumber = string.Empty;
            if (!string.IsNullOrEmpty(act.SecretaryUserId))
            {
                var secretaryName = repo.AllReadonly<ApplicationUser>()
                                        .Include(x => x.LawUnit)
                                        .Where(x => x.Id == act.SecretaryUserId)
                                        .Select(x => x.LawUnit)
                                        .FirstOrDefault()?.FullName_MiddleNameInitials;
                result.SecretaryName = secretaryName;
            }

            if (act.ActTypeId == NomenclatureConstants.ActType.CommandmentProtection || act.ActTypeId == NomenclatureConstants.ActType.CommandmentimmediatelyProtection)
            {
                var actOther = repo.AllReadonly<CaseSessionAct>()
                                   .Where(x => x.CaseSessionId == act.CaseSessionId &&
                                               (x.ActTypeId == NomenclatureConstants.ActType.Answer || x.ActTypeId == NomenclatureConstants.ActType.Definition))
                                   .FirstOrDefault();

                result.AnswerActRegNumber = (actOther != null) ? (actOther.RegNumber + "/" + (actOther.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy")) : string.Empty;
            }

            var lawUnits = caseLawUnitService.CaseLawUnit_Select(act.CaseSession.CaseId, act.CaseSessionId).ToList();
            //var persons = casePersonService.CasePerson_Select(act.CaseSession.CaseId, act.CaseSessionId);
            foreach (var item in lawUnits)
            {
                switch (item.JudgeRoleId)
                {
                    case NomenclatureConstants.JudgeRole.JudgeReporter:
                    case NomenclatureConstants.JudgeRole.Judge:
                    case NomenclatureConstants.JudgeRole.ExtJudge:
                    case NomenclatureConstants.JudgeRole.JudgeVAS:
                        {
                            var newItem = new LabelValueVM()
                            {
                                Value = item.LawUnitNameShort
                            };
                            newItem.Label = repo.AllReadonly<CourtLawUnit>()
                                                    .Include(x => x.Court)
                                                    .Where(x => x.LawUnitId == item.LawUnitId)
                                                    .Where(x => x.PeriodTypeId == NomenclatureConstants.PeriodTypes.Appoint)
                                                    .Where(x => x.DateFrom <= act.ActDate && (x.DateTo ?? DateTime.MaxValue) >= act.ActDate)
                                                    .Select(x => x.Court.Label)
                                                    .FirstOrDefault();

                            if (item.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                            {
                                result.JudgeReporter = item.LawUnitNameShort;
                            }
                            else
                            {
                                result.JudgeList.Add(newItem);
                            }

                            if (item.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                            {
                                result.JudgeChairman = item.LawUnitNameShort;
                            }
                            else
                            {
                                result.AllJudgeList.Add(newItem);
                            }
                        }
                        break;
                    case NomenclatureConstants.JudgeRole.Jury:
                    case NomenclatureConstants.JudgeRole.ReserveJury:
                    case NomenclatureConstants.JudgeRole.ExtJury:
                        result.JuryList.Add(item.LawUnitName);
                        break;
                }
            }
            var depInfo = repo.AllReadonly<CaseLawUnit>()
                                    //.Include(x => x.CourtDepartment)
                                    //.ThenInclude(x => x.ParentDepartment)
                                    //.ThenInclude(x => x.DepartmentType)
                                    .Where(x => x.CaseSessionId == null)
                                    .Where(x => x.CourtDepartmentId > 0)
                                    .Where(x => x.CaseId == result.CaseId)
                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                    .Where(x => x.DateFrom <= actDate && (x.DateTo ?? DateTime.MaxValue) > actDate)
                                    .Where(x => !NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId))
                                    .Select(x => new
                                    {
                                        DepartmentName = x.CourtDepartment.Label,
                                        CompartmentType = (x.CourtDepartment.ParentDepartment != null) ? x.CourtDepartment.ParentDepartment.DepartmentType.Label : "",
                                        CompartmentName = (x.CourtDepartment.ParentDepartment != null) ? x.CourtDepartment.ParentDepartment.Label : ""
                                    })
                                    .FirstOrDefault();
            if (depInfo != null)
            {
                result.DepartmentName = depInfo.DepartmentName;
                result.CompartmentType = depInfo.CompartmentType;
                result.CompartmentName = depInfo.CompartmentName;
            }



            var casePersonList = casePersonService.CasePerson_Select(act.CaseSession.CaseId, act.CaseSessionId, true).ToList();
            foreach (var casePerson in casePersonList)
            {
                if (casePerson.PersonRoleId == NomenclatureConstants.PersonRole.Prokuror)
                {
                    result.ProsecutorList.Add(casePerson.FullName);
                }
            }
            FillLeftRightSide_CasePersons(casePersonList, result);

            return result;
        }

        /// <summary>
        /// Извличане на тип акт по дело
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="addDefaultElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetActTypesByCase(int caseSessionId, bool addDefaultElement = true)
        {
            var caseInfo = repo.AllReadonly<CaseSession>()
                               .Include(x => x.Case)
                               .ThenInclude(x => x.Court)
                               .Include(x => x.Case)
                               .ThenInclude(x => x.CaseType)
                               .Include(x => x.SessionType)
                               .Where(x => x.Id == caseSessionId)
                               .Select(x => new
                               {
                                   CourtTypeId = x.Case.Court.CourtTypeId,
                                   CaseInstanceId = x.Case.CaseType.CaseInstanceId,
                                   CaseGroupId = x.Case.CaseGroupId,
                                   SessionTypeGroup = x.SessionType.SessionTypeGroup
                               }).FirstOrDefault();

            var actTypes = repo.AllReadonly<ActTypeCourtLink>()
                               .Where(x => x.CaseGroupId == caseInfo.CaseGroupId &&
                                           x.CourtTypeId == caseInfo.CourtTypeId &&
                                           x.CaseInstanceId == caseInfo.CaseInstanceId)
                               .Select(x => x.ActTypeId)
                               .ToArray();

            var selectListItems = repo.AllReadonly<ActTypeSessionTypeGroup>()
                                      .Include(x => x.ActType)
                                      .Where(x => actTypes.Contains(x.ActTypeId) &&
                                                  x.SessionTypeGroup == caseInfo.SessionTypeGroup)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.ActType.Label,
                                          Value = x.ActType.Id.ToString()
                                      })
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        public List<SelectListItem> GetActTypesFromCaseByCase(int caseId, int SessionTypeId, bool addDefaultElement = true)
        {
            var caseInfo = repo.AllReadonly<Case>()
                               .Include(x => x.Court)
                               .Include(x => x.CaseType)
                               .Where(x => x.Id == caseId)
                               .Select(x => new
                               {
                                   CourtTypeId = x.Court.CourtTypeId,
                                   CaseInstanceId = x.CaseType.CaseInstanceId,
                                   CaseGroupId = x.CaseGroupId
                               }).FirstOrDefault();

            var actTypes = repo.AllReadonly<ActTypeCourtLink>()
                               .Where(x => x.CaseGroupId == caseInfo.CaseGroupId &&
                                           x.CourtTypeId == caseInfo.CourtTypeId &&
                                           x.CaseInstanceId == caseInfo.CaseInstanceId)
                               .Select(x => x.ActTypeId)
                               .ToArray();

            var sessionType = repo.GetById<SessionType>(SessionTypeId) ?? new SessionType();

            var selectListItems = repo.AllReadonly<ActTypeSessionTypeGroup>()
                                      .Include(x => x.ActType)
                                      .Where(x => actTypes.Contains(x.ActTypeId) &&
                                                  x.SessionTypeGroup == sessionType.SessionTypeGroup)
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.ActType.Label,
                                          Value = x.ActType.Id.ToString()
                                      })
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Принтиране на заповеди
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CaseSessionActCommandVM CaseSessionActCommand_GetForPrint(int id)
        {
            CaseSessionActCommandVM result = new CaseSessionActCommandVM();
            result.CaseSessionActPrint = CaseSessionAct_GetForPrint(id);
            result.CaseFastProcessView = caseFastProcessService.Select(result.CaseSessionActPrint.CaseId);

            return result;
        }

        /// <summary>
        /// Извличане на вид акт по тип
        /// </summary>
        /// <param name="actTypeId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetActKindsByActType(int actTypeId)
        {
            return repo.AllReadonly<ActKind>()
                             .Where(x => x.ActTypeId == actTypeId)
                             .ToSelectList(true);
        }

        /// <summary>
        /// Извличане на актове за архивиране
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownListForArchive(int caseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Include(x => x.CaseSession)
                             .Include(x => x.ActType)
                             .Include(x => x.ActState)
                             .Where(x => x.CaseSession.CaseId == caseId)
                             .Where(x => x.ActStateId != NomenclatureConstants.SessionActState.Project)
                .Select(x => new SelectListItem()
                {
                    Text = x.ActType.Label + " " + x.ActState.Label + " " + (x.RegNumber ?? string.Empty) + ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                    Value = x.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Генериране на екли номер
        /// </summary>
        /// <param name="model"></param>
        private void GenerateActEcliNumber(CaseSessionAct model)
        {
            if (!model.IsFinalDoc)
            {
                return;
            }

            var actInfo = repo.AllReadonly<CaseSession>()
                                .Where(this.FilterExpireInfo<CaseSession>(false))
                                .Where(x => x.Id == model.CaseSessionId)
                                .Select(x => new
                                {
                                    CaseId = x.CaseId
                                })
                                .FirstOrDefault();

            if (actInfo == null)
            {
                return;
            }

            var countFinalActs = repo.AllReadonly<CaseSessionAct>()
                                .Include(x => x.CaseSession)
                                .Where(this.FilterExpireInfo<CaseSessionAct>(false))
                                .Where(x => x.CaseSession.CaseId == actInfo.CaseId)
                                .Where(x => x.IsFinalDoc == true)
                                .Count();

            var caseInfo = repo.AllReadonly<Case>()
                                .Include(x => x.Court)
                                .Include(x => x.CaseCharacter)
                                .Where(x => x.Id == actInfo.CaseId)
                                .Select(x => new
                                {
                                    Year = x.RegDate.Year,
                                    ShortNumber = x.ShortNumberValue,
                                    CourtCode = x.Court.EcliCode,
                                    CharacterCode = x.CaseCharacter.Code
                                })
                                .FirstOrDefault();

            string result = $"ECLI:BG:{caseInfo.CourtCode}:{model.ActDate.Value.Year:D4}:{caseInfo.Year:D4}{caseInfo.CharacterCode}{caseInfo.ShortNumber:D5}.{countFinalActs:D3}";

            model.EcliCode = result;
        }

        /// <summary>
        /// Автоматично обезличаване на съдебни актове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IEnumerable<DepersonalizationHistoryItem> AutoDepersonalizeAct_GenerateRules(CaseSessionAct model)
        {
            var _case = model.CaseSession.Case;
            List<DepersonalizationHistoryItem> rules = new List<DepersonalizationHistoryItem>();
            //Добавяне на адреси на лица
            foreach (var _person in _case.CasePersons.Where(x => x.CaseSessionId == null))
            {
                if (_person.Addresses != null)
                    foreach (var _adr in _person.Addresses)
                    {
                        rules.Add(new DepersonalizationHistoryItem()
                        {
                            SearchValue = _adr.Address.FullAddress
                        });
                        if (!string.IsNullOrEmpty(_adr.Address.Email))
                        {
                            rules.Add(new DepersonalizationHistoryItem()
                            {
                                SearchValue = _adr.Address.Email
                            });
                        }
                        if (!string.IsNullOrEmpty(_adr.Address.Phone))
                        {
                            rules.Add(new DepersonalizationHistoryItem()
                            {
                                SearchValue = _adr.Address.Phone
                            });
                        }
                    }
            }
            //Добавяне на страни
            foreach (var _person in _case.CasePersons.Where(x => x.CaseSessionId == null))
            {
                //Само за физически лица
                if (!(_person.UicTypeId == NomenclatureConstants.UicTypes.EGN
                    || _person.UicTypeId == NomenclatureConstants.UicTypes.LNCh
                    || _person.UicTypeId == NomenclatureConstants.UicTypes.BirthDate))
                {
                    continue;
                }
                rules.Add(new DepersonalizationHistoryItem()
                {
                    SearchValue = _person.FullName,
                    ReplaceValue = _person.FullName_Initials
                });
                if (!string.IsNullOrEmpty(_person.Uic))
                {
                    rules.Add(new DepersonalizationHistoryItem()
                    {
                        SearchValue = _person.Uic
                    });
                }
            }
            return rules.Where(x => !string.IsNullOrEmpty(x.SearchValue));
        }

        /// <summary>
        /// Обезличаване на съдебни актове
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public string AutoDepersonalizeAct(IEnumerable<DepersonalizationHistoryItem> rules, string html)
        {
            string result = html;
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            foreach (var item in rules)
            {
                item.ReplaceValue = item.ReplaceValue ?? "**********";
                item.IsCaseSensitive = false;

                result = result.Replace(item.SearchValue, item.ReplaceValue, item.CaseSensitivity);
            }

            // caseService.SaveDataDepersonalizationHistory(model.CaseId, replaceItems, int.Parse(model.SourceId));

            return result;
        }

        /// <summary>
        /// Извличане на разводи по ид на акт
        /// </summary>
        /// <param name="actId"></param>
        /// <returns></returns>
        public CaseSessionActDivorce GetDivorceByActId(int actId)
        {
            return repo.AllReadonly<CaseSessionActDivorce>().Where(x => x.CaseSessionActId == actId).FirstOrDefault();
        }

        /// <summary>
        /// Запис на развод
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CaseSessionActDivorce_SaveData(CaseSessionActDivorce model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var exists = repo.AllReadonly<CaseSessionActDivorce>()
                                           .Where(x => x.Id != model.Id)
                                           .Where(x => x.CaseSessionActId == model.CaseSessionActId)
                                           .Any();
                    if (exists == true)
                    {
                        return (result: false, errorMessage: "Вече има въведени данни");
                    }
                }

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActDivorce>(model.Id);

                    saved.CountryCode = model.CountryCode;
                    saved.CountryCodeDate = model.CountryCodeDate;
                    saved.MarriageNumber = model.MarriageNumber;
                    saved.MarriageDate = model.MarriageDate;
                    saved.MarriagePlace = model.MarriagePlace;
                    saved.MarriageFault = model.MarriageFault;
                    saved.MarriageFaultDescription = model.MarriageFaultDescription;
                    saved.ChildrenUnder18 = model.ChildrenUnder18;
                    saved.ChildrenOver18 = model.ChildrenOver18;
                    saved.CasePersonManId = model.CasePersonManId;
                    saved.BirthDayMan = model.BirthDayMan;
                    saved.NameAfterMarriageMan = model.NameAfterMarriageMan;
                    saved.MarriedStatusBeforeMan = model.MarriedStatusBeforeMan;
                    saved.MarriageCountMan = model.MarriageCountMan;
                    saved.DivorceCountMan = model.DivorceCountMan;
                    saved.NationalityMan = model.NationalityMan;
                    saved.EducationMan = model.EducationMan;
                    saved.CasePersonWomanId = model.CasePersonWomanId;
                    saved.BirthDayWoman = model.BirthDayWoman;
                    saved.NameAfterMarriageWoman = model.NameAfterMarriageWoman;
                    saved.MarriedStatusBeforeWoman = model.MarriedStatusBeforeWoman;
                    saved.MarriageCountWoman = model.MarriageCountWoman;
                    saved.DivorceCountWoman = model.DivorceCountWoman;
                    saved.NationalityWoman = model.NationalityWoman;
                    saved.EducationWoman = model.EducationWoman;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    if (counterService.Counter_GetDivorceCounter(model, userContext.CourtId) == false)
                    {
                        return (result: false, errorMessage: "Проблем при вземане на номер");
                    }

                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<CaseSessionActDivorce>(model);
                }

                repo.SaveChanges();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseSessionActDivorce Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на актове за комбо
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDropDownList_CaseSessionAct(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseSessionAct>()
                             .Include(x => x.CaseSession)
                             .Include(x => x.ActType)
                             .Where(x => x.CaseSession.CaseId == CaseId &&
                                         x.IsFinalDoc == true)
                             .Select(x => new SelectListItem()
                             {
                                 Text = x.ActType.Label + " - " + x.RegNumber + "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy"),
                                 Value = x.Id.ToString()
                             })
                             .ToList();

            if (result.Count != 1)
            {
                if (addDefaultElement)
                {
                    result = result
                        .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                        .ToList();
                }

                if (addAllElement)
                {
                    result = result
                        .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                        .ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Извличане на актове от движение на дело за комбо
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="CourtId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseSessionActFromMigration(int CaseId, int CourtId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = new List<SelectListItem>();

            var caseMigrationFind = repo.AllReadonly<CaseMigration>().Where(x => x.CaseId == CaseId).FirstOrDefault();

            if (caseMigrationFind != null)
            {
                var caseMigrations = repo.AllReadonly<CaseMigration>()
                                         .Include(x => x.Case)
                                         .ThenInclude(x => x.Court)
                                         .Where(x => x.InitialCaseId == caseMigrationFind.InitialCaseId && x.Case.CourtId == CourtId)
                                         .ToList();
                var caseIdList = caseMigrations.Select(x => x.CaseId).Distinct().ToList();

                foreach (var _caseId in caseIdList)
                {
                    result.AddRange(repo.AllReadonly<CaseSessionAct>()
                                        .Include(x => x.CaseSession)
                                        .ThenInclude(x => x.Case)
                                        .ThenInclude(x => x.Court)
                                        .Include(x => x.ActType)
                                        .Include(x => x.ActState)
                                        .Where(x => x.CaseSession.CaseId == _caseId && x.IsFinalDoc)
                                        .Select(x => new SelectListItem()
                                        {
                                            Text = x.ActType.Label + " " + (x.RegNumber ?? string.Empty) +
                                                   ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) +
                                                   " Дело: " + x.CaseSession.Case.RegNumber + "/" + x.CaseSession.Case.RegDate.ToString("dd.MM.yyyy"),
                                            Value = x.Id.ToString()
                                        }).ToList() ?? new List<SelectListItem>());
                }
            }
            else
            {
                result.AddRange(repo.AllReadonly<CaseSessionAct>()
                                    .Include(x => x.CaseSession)
                                    .ThenInclude(x => x.Case)
                                    .ThenInclude(x => x.Court)
                                    .Include(x => x.ActType)
                                    .Include(x => x.ActState)
                                    .Where(x => x.CaseSession.CaseId == CaseId && x.IsFinalDoc)
                                    .Select(x => new SelectListItem()
                                    {
                                        Text = x.ActType.Label + " " + (x.RegNumber ?? string.Empty) +
                                               ((x.RegDate != null) ? "/" + (x.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) +
                                               " Дело: " + x.CaseSession.Case.RegNumber + "/" + x.CaseSession.Case.RegDate.ToString("dd.MM.yyyy"),
                                        Value = x.Id.ToString()
                                    }).ToList() ?? new List<SelectListItem>());
            }

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Справка за изпълнителни листове
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActELSprVM> CaseSessionActELSpr_Select(int courtId, CaseSessionActELSprFilterVM model)
        {
            DateTime fromDateNull = (model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom).Date;
            DateTime toDateNull = (model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var caseSessionActELSprVMs = repo.AllReadonly<CaseSessionAct>()
                                             .Include(x => x.ActKind)
                                             .Include(x => x.CaseSession)
                                             .ThenInclude(x => x.Case)
                                             .ThenInclude(x => x.CasePersons)
                                             .ThenInclude(x => x.PersonRole)
                                             .Where(x => (x.CaseSession.Case.CourtId == courtId) &&
                                                         ((fromDateNull <= x.RegDate) && (x.RegDate <= toDateNull)) &&
                                                         (x.ActTypeId == NomenclatureConstants.ActType.ExecListPrivatePerson) &&
                                                         ((model.ActKindId > 0) ? x.ActKindId == model.ActKindId : true) &&
                                                         (!string.IsNullOrEmpty(model.RegNumber) ? x.RegNumber.ToLower().Contains((model.RegNumber ?? string.Empty).ToLower()) : true) &&
                                                         ((!string.IsNullOrEmpty(model.LeftSide)) ? x.CaseSession.Case.CasePersons.Where(p => p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide && p.CaseSessionId == null).Any(p => p.FullName.ToLower().Contains((model.LeftSide ?? string.Empty).ToLower())) : true) &&
                                                         ((!string.IsNullOrEmpty(model.RightSide)) ? x.CaseSession.Case.CasePersons.Where(p => p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide && p.CaseSessionId == null).Any(p => p.FullName.ToLower().Contains((model.RightSide ?? string.Empty).ToLower())) : true))
                                             .Select(x => new CaseSessionActELSprVM()
                                             {
                                                 Id = x.Id,
                                                 CaseId = x.CaseSession.CaseId,
                                                 RegNumber = x.RegNumber,
                                                 RegDate = (x.RegDate ?? DateTime.Now),
                                                 LeftSide = string.Join(", ", x.CaseSession.Case.CasePersons.Where(p => p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide && p.CaseSessionId == null).Select(p => p.FullName)),
                                                 RightSide = string.Join(", ", x.CaseSession.Case.CasePersons.Where(p => p.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.RightSide && p.CaseSessionId == null).Select(p => p.FullName)),
                                                 ActKindName = x.ActKind.Label
                                             })
                                             .ToList();

            return caseSessionActELSprVMs.AsQueryable();
        }

        /// <summary>
        /// Извличане на актове за обжалване
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_FinalActToApeal(int caseId)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var caseSessionActs = repo.AllReadonly<CaseSessionActComplain>()
                                      .Where(x => x.CaseId == caseId && x.DateExpired == null)
                                      .Select(x => x.CaseSessionAct)
                                      .Include(x => x.ActType)
                                      .Where(x => x.CaseId == caseId && x.IsFinalDoc && (x.CanAppeal == true))
                                      .Where(x => x.RegDate != null)
                                      .OrderByDescending(x => x.RegDate)
                                      .ToList();

            foreach (var caseSessionAct in caseSessionActs)
            {
                if (!result.Any(x => x.Value == caseSessionAct.Id.ToString()))
                {
                    var act = new SelectListItem
                    {
                        Value = caseSessionAct.Id.ToString(),
                        Text = $"{caseSessionAct.ActType.Label} {caseSessionAct.RegNumber}/{caseSessionAct.RegDate:dd.MM.yyyy}"
                    };

                    result.Add(act);
                }
            };

            result = result.Prepend(new SelectListItem() { Value = "-1", Text = "Изберете" }).ToList();
            return result;
        }

        /// <summary>
        /// Справка за актове
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActReportVM> CaseSessionActReport_Select(int courtId, CaseSessionActReportFilterVM model)
        {
            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);

            return repo.AllReadonly<CaseSessionAct>()
                       .Include(x => x.ActType)
                       .Include(x => x.ActKind)
                       .Include(x => x.Case)
                       .ThenInclude(x => x.CaseType)
                       .Include(x => x.Case)
                       .ThenInclude(x => x.CaseGroup)
                       .Include(x => x.Case)
                       .ThenInclude(x => x.Document)
                       .ThenInclude(x => x.DocumentType)
                       .Where(x => (x.CourtId == courtId) &&
                                   ((x.RegDate >= model.DateFrom) && (x.RegDate <= model.DateTo)) &&
                                   (model.CaseGroupId > 0 ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                   (model.CaseTypeId > 0 ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                   (model.ActTypeId > 0 ? x.ActTypeId == model.ActTypeId : true) &&
                                   (model.DocumentGroupId > 0 ? x.Case.Document.DocumentGroupId == model.DocumentGroupId : true) &&
                                   (model.DocumentTypeId > 0 ? x.Case.Document.DocumentTypeId == model.DocumentTypeId : true) &&
                                   (model.ActComplainResultId > 0 ? x.ActComplainResultId == model.ActComplainResultId : true) &&
                                   ((model.JudgeReporterId > 0) ? (x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.CaseSession.DateFrom.Date && a.LawUnitId == model.JudgeReporterId &&
                                                                                                          a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any()) : true))
                       .Select(x => new CaseSessionActReportVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId ?? 0,
                           ActRegNumYear = x.RegNumber + "/" + (x.RegDate ?? DateTime.Now).Year + "г.",
                           ActTypeLabel = x.ActType.Label,
                           RegDate = x.RegDate,
                           ReturnDate = x.ActDate,
                           ActInforcedDate = x.ActInforcedDate,
                           CaseActInfoLabel = x.Case.CaseType.Code + " " + x.Case.RegNumber,
                           DocumentInfo = x.Case.Document.DocumentType.Label + " " + x.Case.Document.DocumentNumber + "/" + x.Case.Document.DocumentDate.ToString("dd.MM.yyyy"),
                           ActStateName = x.ActState.Label
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Вземане на последне подписал
        /// </summary>
        /// <param name="caseLawunitid"></param>
        /// <param name="actId"></param>
        /// <returns></returns>
        public DateTime GetLastSignCaseDate(int caseLawunitid, int? actId)
        {
            var caseLawUnit = repo.GetById<CaseLawUnit>(caseLawunitid);
            Expression<Func<CaseSessionAct, bool>> selectAct = x => true;
            if (actId != null)
            {
                selectAct = x => x.Id == actId;

            }
            var lastDate = repo.AllReadonly<CaseSessionAct>()
                            .Where(selectAct)
                            .Where(x => x.CaseId == caseLawUnit.CaseId)
                            .Select(x => x.ActDeclaredDate).Max();


            return (lastDate ?? DateTime.Now.AddYears(-50));
        }

        /// <summary>
        /// Извличане на информация за Данни за регистрация на фирма
        /// </summary>
        /// <param name="actId"></param>
        /// <returns></returns>
        public CaseSessionActCompany GetCompanyByActId(int actId)
        {
            return repo.AllReadonly<CaseSessionActCompany>().Where(x => x.CaseSessionActId == actId).FirstOrDefault();
        }

        /// <summary>
        /// Запис на Данни за регистрация на фирма
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public (bool result, string errorMessage) CaseSessionActCompany_SaveData(CaseSessionActCompany model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var exists = repo.AllReadonly<CaseSessionActCompany>()
                                           .Where(x => x.Id != model.Id)
                                           .Where(x => x.CaseSessionActId == model.CaseSessionActId)
                                           .Any();
                    if (exists == true)
                    {
                        return (result: false, errorMessage: "Вече има въведени данни");
                    }
                }

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActCompany>(model.Id);

                    saved.RegisterDate = model.RegisterDate;
                    saved.RegisterNumber = model.RegisterNumber;
                    saved.Chapter = model.Chapter;
                    saved.PageNumber = model.PageNumber;
                    saved.Batch = model.Batch;
                    saved.Level = model.Level;
                    saved.Authorization = model.Authorization;
                    saved.UserId = userContext.UserId;
                    saved.DateWrt = DateTime.Now;
                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    model.UserId = userContext.UserId;
                    model.DateWrt = DateTime.Now;
                    repo.Add<CaseSessionActCompany>(model);
                }

                repo.SaveChanges();
                return (result: true, errorMessage: "");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на CaseSessionActCompany Id={ model.Id }");
                return (result: false, errorMessage: Helper.GlobalConstants.MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Извличане на данни за акт
        /// </summary>
        /// <param name="actId"></param>
        /// <returns></returns>
        public CaseSessionAct GetByIdWithOtherData(int actId)
        {
            return repo.AllReadonly<CaseSessionAct>()
                       .Include(x => x.ActType)
                       .Where(x => x.Id == actId)
                       .FirstOrDefault();
        }

        /// <summary>
        /// Извличане на актове от дело за обжалване за комбо
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CanAppealAct(int caseId)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var caseSessionActs = repo.AllReadonly<CaseSessionActComplain>()
                                      .Where(x => x.CaseId == caseId && x.DateExpired == null)
                                      .Select(x => x.CaseSessionAct)
                                      .Include(x => x.ActType)
                                      .Where(x => x.CaseId == caseId && (x.CanAppeal == true))
                                      .Where(x => x.RegDate != null)
                                      .OrderByDescending(x => x.RegDate)
                                      .ToList();

            foreach (var caseSessionAct in caseSessionActs)
            {
                if (!result.Any(x => x.Value == caseSessionAct.Id.ToString()))
                {
                    var act = new SelectListItem
                    {
                        Value = caseSessionAct.Id.ToString(),
                        Text = $"{caseSessionAct.ActType.Label} {caseSessionAct.RegNumber}/{caseSessionAct.RegDate:dd.MM.yyyy}"
                    };

                    result.Add(act);
                }
            };

            result = result.Prepend(new SelectListItem() { Value = "-1", Text = "Изберете" }).ToList();
            return result;
        }

        public List<SelectListItem> GetDropDownList_CaseSessionActEnforced(int CaseId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var result = repo.AllReadonly<CaseSessionAct>()

                            .Include(x => x.ActType)
                            .Include(x => x.CaseSession)
                            .ThenInclude(x => x.SessionType)
                            .Where(FilterExpireInfo<CaseSessionAct>(false))
                            .Where(x => x.CaseSession.CaseId == CaseId &&
                                        NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId))
                            .OrderByDescending(x => x.Id)
                            .Select(x => new SelectListItem()
                            {
                                Text = $"{x.ActType.Label} {x.RegNumber} ({x.CaseSession.SessionType.Label} {x.CaseSession.DateFrom:dd.MM.yyyy})",
                                Value = x.Id.ToString()
                            })
                            .ToList();

            if (addDefaultElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                    .ToList();
            }

            if (addAllElement)
            {
                result = result
                    .Prepend(new SelectListItem() { Text = "Всички", Value = "-2" })
                    .ToList();
            }

            return result;
        }

        public List<CaseSessionActVM> GetSessionActsFinal(int CaseId)
        {
            return repo.AllReadonly<CaseSessionAct>()
                       .Where(x => x.CaseId == CaseId &&
                                   x.DateExpired == null &&
                                   x.ActStateId == NomenclatureConstants.SessionActState.ComingIntoForce)
                       .Select(x => new CaseSessionActVM()
                       {
                           Id = x.Id,
                           CaseSessionId = x.CaseSessionId,
                           CaseId = x.CaseSession.CaseId,
                           CaseSessionLabel = (x.CaseSession != null) ? x.CaseSession.SessionType.Label + "/" + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm") : string.Empty,
                           CaseLabel = x.CaseSession.Case.RegNumber + "/" + x.CaseSession.Case.RegDate.ToString("dd.MM.yyyy"),
                           ActTypeLabel = (x.ActType != null) ? x.ActType.Label : string.Empty,
                           ActStateLabel = (x.ActState != null) ? x.ActState.Label : string.Empty,
                           RegNumber = x.RegNumber,
                           RegDate = x.RegDate.Value,
                           IsFinalDoc = x.IsFinalDoc,
                           DateWrt = x.DateWrt,
                           EcliCode = x.EcliCode,
                           Description = x.Description
                       })
                       .ToList();
        }

        /// <summary>
        /// извличане на данни за Справка съдебни актове и протоколи
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActVM> CaseSessionActSpr_Select(int courtId, CaseSessionActFilterVM model)
        {
            DateTime dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100);
            DateTime dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100);

            Expression<Func<CaseSessionAct, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.RegDate >= dateFromSearch.ForceStartDate() && x.RegDate <= dateToSearch.ForceEndDate();

            Expression<Func<CaseSessionAct, bool>> yearSearch = x => true;
            if ((model.Year ?? 0) > 0)
                yearSearch = x => x.Case.RegDate.Year == model.Year;

            Expression<Func<CaseSessionAct, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.CaseRegNumber))
                caseRegnumberSearch = x => x.Case.RegNumber.EndsWith(model.CaseRegNumber.ToShortCaseNumber(), StringComparison.InvariantCultureIgnoreCase);

            Expression<Func<CaseSessionAct, bool>> finalActSearch = x => true;
            if (model.IsFinalDoc == true)
                finalActSearch = x => x.IsFinalDoc == true && NomenclatureConstants.SessionActState.EnforcedStates.Contains(x.ActStateId);

            Expression<Func<CaseSessionAct, bool>> actLawBaseSearch = x => true;
            if (model.ActLawBaseId > 0)
                actLawBaseSearch = x => x.CaseSessionActLawBases.Where(a => a.LawBaseId == model.ActLawBaseId).Any();

            return repo.AllReadonly<CaseSessionAct>()
                .Where(x => x.CourtId == courtId)
                .Where(x => x.DateExpired == null)
                .Where(dateSearch)
                .Where(yearSearch)
                .Where(caseRegnumberSearch)
                .Where(finalActSearch)
                .Where(actLawBaseSearch)
                .Select(x => new CaseSessionActVM()
                {
                    Id = x.Id,
                    CaseSessionId = x.CaseSessionId,
                    CaseId = x.CaseSession.CaseId,
                    CaseSessionLabel = (x.CaseSession != null) ? x.CaseSession.SessionType.Label + "/" + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm") : string.Empty,
                    CaseLabel = x.CaseSession.Case.RegNumber + "/" + x.CaseSession.Case.RegDate.ToString("dd.MM.yyyy"),
                    ActTypeLabel = (x.ActType != null) ? x.ActType.Label : string.Empty,
                    ActStateLabel = (x.ActState != null) ? x.ActState.Label : string.Empty,
                    RegNumber = x.RegNumber,
                    RegDate = x.RegDate.Value,
                })
                .AsQueryable();
        }
    }
}
