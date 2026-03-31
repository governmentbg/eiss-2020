using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Data.Models.VKS;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Report;
using IOWebApplication.Infrastructure.Models.ViewModels.VKSSelection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using static IOWebApplication.Infrastructure.Constants.EpepConstants;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionService : BaseService, ICaseSessionService
    {
        private readonly ICasePersonService casePersonService;
        private readonly ICaseLawUnitService caseLawUnitService;
        private readonly IMoneyService moneyService;
        private readonly ICaseLifecycleService caseLifecycleService;
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly IMQEpepService mqEpepService;
        private readonly ICaseLoadIndexService caseLoadIndexService;
        private readonly ICommonService commonService;
        private readonly IUrlHelper urlHelper;
        private readonly IVksNotificationService vksNotificationService;
        private readonly ICourtLawUnitService courtLawUnitService;
        private readonly IWorkNotificationService workNotificationService;

        public CaseSessionService(ILogger<CaseSessionService> _logger,
                                  IRepository _repo,
                                  IUserContext _userContext,
                                  ICasePersonService _casePersonService,
                                  IMoneyService _moneyService,
                                  ICaseLifecycleService _caseLifecycleService,
                                  ICaseLawUnitService _caseLawUnitService,
                                  IMQEpepService _mqEpepService,
                                  ICaseDeadlineService _caseDeadlineService,
                                  ICaseLoadIndexService _caseLoadIndexService,
                                  ICommonService _commonService,
                                  IUrlHelper _url,
                                  IVksNotificationService _vksNotificationService,
                                  ICourtLawUnitService _courtLawUnitService,
                                  IReadonlyRepository _readonlyrepo,
                                  IConfiguration _config,
                                  IWorkNotificationService _workNotificationService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            casePersonService = _casePersonService;
            moneyService = _moneyService;
            caseLifecycleService = _caseLifecycleService;
            caseLawUnitService = _caseLawUnitService;
            caseDeadlineService = _caseDeadlineService;
            mqEpepService = _mqEpepService;
            caseLoadIndexService = _caseLoadIndexService;
            commonService = _commonService;
            urlHelper = _url;
            vksNotificationService = _vksNotificationService;
            courtLawUnitService = _courtLawUnitService;
            readonlyrepo = _readonlyrepo;
            applicationDbConnectionString = _config.GetConnectionString("DefaultConnection");
            workNotificationService = _workNotificationService;
        }

        /// <summary>
        /// Изчитане на данни за заседания по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="IsVisibleExpired"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionListVM> CaseSession_Select(int CaseId, DateTime? DateFrom, DateTime? DateTo, bool IsVisibleExpired = false)
        {
            List<CaseSessionListVM> result = new List<CaseSessionListVM>();

            var datetimeNow = DateTime.Now;

            DateFrom = DateFrom.ForceStartDateWithAddYear(-100);
            DateTo = DateTo.ForceEndDateWithAddYear(100);

            Expression<Func<CaseSession, bool>> dateFromWhere = x => x.DateFrom >= DateFrom && x.DateFrom <= DateTo;

            return repo.AllReadonly<CaseSession>()
                       .Where(x => x.CaseId == CaseId)
                       .Where(dateFromWhere)
                       .Where(this.FilterExpireInfo<CaseSession>(IsVisibleExpired))
                       .Select(x => new CaseSessionListVM()
                       {
                           Id = x.Id,
                           SessionTypeLabel = x.SessionType.Label,
                           CourtHallName = x.CourtHallId != null ? x.CourtHall.Name : string.Empty,
                           SessionStateLabel = x.SessionState.Label,
                           DateFrom = x.DateFrom,
                           SessionResultLabel = x.CaseSessionResults.Where(r => r.IsMain && r.DateExpired == null).Select(r => r.SessionResult.Label).FirstOrDefault(),
                           ActComplainResultLabel = x.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null && a.ActComplainResultId != null).Select(a => a.ActComplainResult.Label).FirstOrDefault(),
                           SessionWornings = (x.DateFrom <= datetimeNow && (x.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno || x.SessionStateId == NomenclatureConstants.SessionState.Provedeno)) ? ((!x.CaseSessionActs.Any(a => a.DateExpired == null && a.ActDeclaredDate != null) ? NomenclatureConstants.SessionWornings.NotExistAct : ((!x.CaseSessionResults.Any(r => r.DateExpired == null) ? NomenclatureConstants.SessionWornings.NotExistResult : NomenclatureConstants.SessionWornings.AllOk)))) : NomenclatureConstants.SessionWornings.AllOk,
                           SessionActList = string.Join("", x.CaseSessionActs.Where(a => a.DateExpired == null && a.ActDeclaredDate != null).OrderBy(a => a.RegDate).Select(a => "<div class='cdn-listview' style='margin-left:5px;'><a href='#' class='cdn-loader' data-sourceType='" + @SourceTypeSelectVM.CaseSessionActAllFiles + "' data-sourceId='" + a.Id + "'><i class='fa fa-file-text'></i> " + a.ActType.Label + " " + (!string.IsNullOrEmpty(a.RegNumber) ? a.RegNumber + "/" + (a.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) + "</a></div>"))
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Изчитане на данни за заседания по дело
        /// </summary>
        /// <param name="CaseId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="IsVisibleExpired"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionVM> CaseSession_OldSelect(int CaseId, DateTime? DateFrom, DateTime? DateTo, bool IsVisibleExpired = false)
        {
            return repo.AllReadonly<CaseSession>()
                .Where(x => x.CaseId == CaseId &&
                            ((DateFrom != null) ? ((x.DateFrom.Date >= (DateFrom ?? DateTime.Now).Date) && (x.DateFrom.Date <= (DateTo ?? DateTime.Now).Date)) : true)
                            )
                .Where(this.FilterExpireInfo<CaseSession>(IsVisibleExpired))
                .Select(x => new CaseSessionVM()
                {
                    Id = x.Id,
                    CourtId = x.CourtId,
                    CaseId = x.CaseId,
                    CaseName = x.Case.RegNumber,
                    SessionTypeLabel = x.SessionType.Label,
                    CourtHallName = (x.CourtHallId > 0) ? x.CourtHall.Name : string.Empty,
                    SessionStateLabel = x.SessionState.Label,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    VideoUrl = x.VideoUrl,
                    Description = x.Description,
                    CourtHallId = x.CourtHallId,
                    SessionStateId = x.SessionStateId,
                    SessionTypeId = x.SessionTypeId
                })
                .AsQueryable();
        }

        private static void CaseSessionEmptyToNull(CaseSessionVM model)
        {
            model.CourtHallId = model.CourtHallId.EmptyToNull();
            model.ActTypeId = model.ActTypeId.NumberEmptyToNull();
            model.ActKindId = model.ActKindId.NumberEmptyToNull();
            model.ActISPNReasonId = model.ActISPNReasonId.NumberEmptyToNull();
            model.RelatedActId = model.RelatedActId.NumberEmptyToNull();
            model.ActComplainResultId = model.ActComplainResultId.NumberEmptyToNull();
            model.VksLawunitChange = model.VksLawunitChange.NumberEmptyToNull();
            model.SessionResultId = model.SessionResultId.NumberEmptyToNull();
            model.SessionResultBaseId = model.SessionResultBaseId.NumberEmptyToNull();
        }

        /// <summary>
        /// Запис на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<SaveResultVM> CaseSession_SaveData(CaseSessionVM model)
        {
            try
            {
                var dateTimeNow = DateTime.Now;
                var date_session = model.DateFrom.MakeZeroSeconds();
                var date_now = dateTimeNow.MakeZeroSeconds();

                CaseSessionEmptyToNull(model);

                if (!(model.IsFinalDoc ?? false))
                    model.ActComplainResultId = null;

                if (!await CheckCaseFeature(model.CaseId, NomenclatureConstants.CaseFeatures.ISPN_ActHasForRegistration))
                    model.TDActForRegistration = false;

                var selectSession = 0;
                if (model.CaseSessionAddActId > 0)
                {
                    selectSession = model.CaseSessionAddActId ?? 0;
                    model.Id = selectSession;
                }

                CaseSession saved = (model.Id > 0) ? await repo.GetByIdAsync<CaseSession>(model.Id) : new CaseSession();
                var savedState = saved.SessionStateId;

                bool isFastProcess = await repo.GetPropByIdAsync<Case, bool>(x => x.Id == saved.CaseId, x => x.IsFastProcess ?? false);

                if (saved.Id > 0)
                {
                    if (saved.DateWrt > model.DateWrt.AddSeconds(1))
                    {
                        return new SaveResultVM()
                        {
                            Result = false,
                            ReloadNeeded = true
                        };
                    }
                }

                if (selectSession < 1)
                {
                    saved.CaseId = model.CaseId;
                    saved.CourtId = model.CourtId;
                    saved.SessionTypeId = model.SessionTypeId;
                    saved.CourtHallId = model.CourtHallId;
                    saved.VideoUrl = model.VideoUrl;
                    saved.Description = model.Description;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateFrom.AddMinutes(model.DateTo_Minutes);
                    saved.CaseLawunitChange = model.VksLawunitChange;
                }

                if ((model.ActTypeId != null) || (selectSession > 0))
                {
                    if (date_session <= date_now)
                        saved.SessionStateId = NomenclatureConstants.SessionState.Provedeno;
                    else
                        saved.SessionStateId = NomenclatureConstants.SessionState.Nasrocheno;
                }
                else
                {
                    saved.SessionStateId = model.SessionStateId;
                }

                saved.DateWrt = dateTimeNow;
                saved.UserId = userContext.UserId;

                if (userContext.IsSystemInFeature(NomenclatureConstants.SystemFeatures.ClearTrackedUsers))
                {
                    try
                    {
                        repo.StopTrackingApplicationUser();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "StopTrackingApplicationUser.CaseNotification_SaveData");
                    }
                }

                if (model.Id > 0)
                {
                    using (var ts = repo.BeginTransaction())
                    {
                        CreateHistory<CaseSession, CaseSessionH>(saved);
                        caseDeadlineService.DeadLineOnSession(saved);

                        if (selectSession < 1)
                        {
                            if ((model.SessionStateId != NomenclatureConstants.SessionState.Provedeno) && (model.DateFrom.Date >= dateTimeNow.Date))
                            {
                                CaseSessionMeeting caseSessionMeetings = await repo.All<CaseSessionMeeting>()
                                                                                   .Where(x => x.CaseSessionId == saved.Id &&
                                                                                               (x.IsAutoCreate ?? false))
                                                                                   .FirstOrDefaultAsync();
                                if (caseSessionMeetings != null)
                                {
                                    if ((caseSessionMeetings.DateFrom != model.DateFrom) ||
                                        (caseSessionMeetings.DateTo != model.DateTo) ||
                                        (caseSessionMeetings.CourtHallId != model.CourtHallId))
                                    {
                                        DateTime dateTimeToMeeteng = (saved.DateFrom.Date > (saved.DateTo ?? saved.DateFrom).Date) ? saved.DateFrom.MakeEndDate() : (saved.DateTo ?? saved.DateFrom);
                                        caseSessionMeetings.DateFrom = saved.DateFrom;
                                        caseSessionMeetings.DateTo = dateTimeToMeeteng;
                                        caseSessionMeetings.CourtHallId = saved.CourtHallId;
                                        caseSessionMeetings.DateWrt = dateTimeNow;
                                        caseSessionMeetings.UserId = userContext.UserId;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (model.ActTypeId != null)
                            {
                                var caseSessionAct = new CaseSessionAct()
                                {
                                    CourtId = saved.CourtId,
                                    CaseId = saved.CaseId,
                                    CaseSessionId = saved.Id,
                                    ActTypeId = model.ActTypeId ?? 0,
                                    ActKindId = model.ActKindId,
                                    RelatedActId = model.RelatedActId,
                                    ActStateId = NomenclatureConstants.SessionActState.Project,
                                    ActISPNReasonId = model.ActISPNReasonId.EmptyToNull(),
                                    RnflEffectiveImmediately = model.RnflEffectiveImmediately,
                                    IsFinalDoc = false,
                                    TDActForRegistration = model.TDActForRegistration,
                                    IsReadyForPublish = false,
                                    CanAppeal = model.ActCanAppeal,
                                    DateWrt = dateTimeNow,
                                    UserId = userContext.UserId
                                };

                                await repo.AddAsync<CaseSessionAct>(caseSessionAct);
                                await repo.SaveChangesAsync();

                                model.ActSaveId = caseSessionAct.Id;

                                if ((model.SessionResultId > 0) && (date_session <= date_now))
                                {
                                    var caseSessionResult = new CaseSessionResult()
                                    {
                                        CourtId = saved.CourtId,
                                        CaseId = saved.CaseId,
                                        CaseSessionId = saved.Id,
                                        SessionResultId = model.SessionResultId ?? 0,
                                        SessionResultBaseId = model.SessionResultBaseId,
                                        Description = model.Description,
                                        IsActive = true,
                                        IsMain = model.IsMainResult,
                                    };

                                    await repo.AddAsync(caseSessionResult);
                                    AutoChangeCaseStatsFromSessionResult(model.SessionResultId ?? 0, saved.CaseId, null);
                                }
                            }
                        }

                        await repo.SaveChangesAsync();

                        //Запис на пари за заседатели
                        (bool result, string errorMessage) = moneyService.CalcEarningsJury(saved, userContext.CourtId);
                        if (result == false)
                        {
                            return new SaveResultVM(false, errorMessage);
                        }

                        mqEpepService.AppendCaseSession(saved, EpepConstants.ServiceMethod.Update);
                        caseLoadIndexService.EditSessionAndRecalcCase(saved.CaseId, saved.Id);

                        //CBorisoff,29.07.2021
                        //когато заседанието е насрочено и се промени на проведено 
                        //се изпращат всички постановени актове в него към външните системи
                        if (savedState == NomenclatureConstants.SessionState.Nasrocheno
                            && model.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                        {
                            await mqEpepService.AppendActsFromSession(model.Id);
                        }
                        ts.Commit();
                    }
                }
                else
                {
                    var isFirstSession = !await repo.AllReadonly<CaseSession>()
                                                    .Where(FilterExpireInfo<CaseSession>(false))
                                                    .Where(x => x.CaseId == saved.CaseId)
                                                    .AnyAsync();

                    using (var ts = repo.BeginTransaction())
                    {

                        CreateHistory<CaseSession, CaseSessionH>(saved);
                        await repo.AddAsync<CaseSession>(saved);
                        await repo.SaveChangesAsync();

                        var hasVKSLawunitChange = false;

                        //Ако е първо заседание или е избрано заседание с промяна на състава - изтегля нов състав от графика на НД ВКС
                        if (isFirstSession || (saved.CaseLawunitChange == NomenclatureConstants.VksSessionLawunitChange.WithChange))
                        {
                            hasVKSLawunitChange = updateCaseLawUnitByDateVKS(saved);
                        }

                        DateTime dateEnd = dateTimeNow.AddYears(100);
                        List<CaseLawUnit> lawUnits = (model.SessionTypeId == NomenclatureConstants.SessionType.OpenSession ||
                                                      model.SessionTypeId == NomenclatureConstants.SessionType.OpenSessionOpenDoors ||
                                                      model.SessionTypeId == NomenclatureConstants.SessionType.OpenSessionAfterFirst ||
                                                      model.SessionTypeId == NomenclatureConstants.SessionType.DispositionalSession) ? await repo.AllReadonly<CaseLawUnit>()
                                                                                                                                                 .Where(x => (x.CaseId == model.CaseId) &&
                                                                                                                                                                 (x.CaseSessionId == null) &&
                                                                                                                                                                 ((NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) ||
                                                                                                                                                                   NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId) ||
                                                                                                                                                                   x.JudgeRoleId == NomenclatureConstants.JudgeRole.Jury) &&
                                                                                                                                                                 (x.DateFrom <= saved.DateFrom) &&
                                                                                                                                                                 ((x.DateTo ?? dateEnd) >= saved.DateFrom))
                                                                                                                                                 .ToListAsync() :
                                                                                                                                       await repo.AllReadonly<CaseLawUnit>()
                                                                                                                                                 .Where(x => (x.CaseId == model.CaseId) &&
                                                                                                                                                             (x.CaseSessionId == null) &&
                                                                                                                                                             ((NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) ||
                                                                                                                                                               NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                                                                                                                                             (x.DateFrom <= saved.DateFrom) &&
                                                                                                                                                             ((x.DateTo ?? dateEnd) >= saved.DateFrom))
                                                                                                                                                 .ToListAsync();

                        List<CasePerson> casePersons = new();
                        if (model.CaseSessionOldId != null)
                        {
                            casePersons = await repo.AllReadonly<CasePerson>()
                                                    .Include(x => x.Addresses)
                                                    .ThenInclude(x => x.Address)
                                                    .Where(x => (x.CaseId == model.CaseId) &&
                                                                (x.CaseSessionId == model.CaseSessionOldId) &&
                                                                (x.DateExpired == null) &&
                                                                (x.DateFrom <= saved.DateFrom) &&
                                                                (((x.DateTo ?? dateEnd) >= saved.DateFrom)))
                                                    .ToListAsync();
                        }
                        else
                        {
                            casePersons = await repo.AllReadonly<CasePerson>()
                                                    .Include(x => x.Addresses)
                                                    .ThenInclude(x => x.Address)
                                                    .Where(x => (x.CaseId == model.CaseId) &&
                                                                (x.CaseSessionId == null) &&
                                                                (x.DateExpired == null) &&
                                                                (x.DateFrom <= saved.DateFrom) &&
                                                                (((x.DateTo ?? dateEnd) >= saved.DateFrom)))
                                                    .ToListAsync();
                        }

                        List<int> selectLawUnit = new();
                        if ((model.ActTypeId == NomenclatureConstants.ActType.Injunction) ||
                            (model.ActTypeId == NomenclatureConstants.ActType.ObezpechitelnaZapoved))
                        {
                            if (model.CaseLawUnitByCase != null)
                            {
                                if (model.CaseLawUnitByCase.Count > 0)
                                {
                                    selectLawUnit = model.CaseLawUnitByCase.Where(x => x.Checked).Select(x => int.Parse(x.Value)).ToList();
                                }
                            }
                        }

                        foreach (var lawUnit in lawUnits.Where(x => selectLawUnit.Count > 0 ? selectLawUnit.Contains(x.Id) : true))
                        {
                            lawUnit.Id = 0;
                            lawUnit.CaseSessionId = saved.Id;
                            //lawUnit.DateFrom = saved.DateFrom;
                            lawUnit.DateWrt = dateTimeNow;
                            lawUnit.UserId = userContext.UserId;
                            await repo.AddAsync<CaseLawUnit>(lawUnit);
                        }

                        casePersonService.SetCasePersonDataForCopySession(model.CaseId, null, saved.Id, casePersons, saved.DateFrom);
                        await repo.SaveChangesAsync();
                        foreach (var item in casePersons)
                            item.PersonRole = await GetReadonlyAsync<PersonRole>(item.PersonRoleId);

                        List<CaseSessionNotificationList> sessionNotificationList = new();
                        int maxNumber = 0;

                        foreach (var casePerson in casePersons.OrderBy(x => x.RowNumber))
                        {
                            maxNumber++;
                            CaseSessionNotificationList notificationList = new()
                            {
                                CourtId = saved.CourtId,
                                CaseId = saved.CaseId,
                                CaseSessionId = saved.Id,
                                NotificationPersonType = NomenclatureConstants.NotificationPersonType.CasePerson,
                                CasePersonId = casePerson.Id,
                                NotificationAddressId = ((casePerson.Addresses.Count > 0) ? ((casePerson.Addresses.Any(x => (x.ForNotification ?? false) == true)) ? (casePerson.Addresses.Where(x => x.ForNotification == true).FirstOrDefault().AddressId) : (casePerson.Addresses.FirstOrDefault().AddressId)) : (long?)null),
                                RowNumber = maxNumber,
                                DateWrt = dateTimeNow,
                                UserId = userContext.UserId
                            };

                            sessionNotificationList.Add(notificationList);
                        }

                        repo.AddRange(sessionNotificationList);

                        DateTime dateTimeToMeeteng = (saved.DateFrom.Date > (saved.DateTo ?? saved.DateFrom).Date) ? saved.DateFrom.MakeEndDate() : (saved.DateTo ?? saved.DateFrom);

                        var caseSessionMeeting = new CaseSessionMeeting()
                        {
                            CourtId = saved.CourtId,
                            CaseId = saved.CaseId,
                            CaseSessionId = saved.Id,
                            SessionMeetingTypeId = NomenclatureConstants.SessionMeetingType.PublicMeeting,
                            DateFrom = saved.DateFrom,
                            DateTo = dateTimeToMeeteng,
                            CourtHallId = saved.CourtHallId,
                            Description = "Автоматично създаване",
                            IsAutoCreate = true,
                            IsActive = true,
                            DateWrt = dateTimeNow,
                            UserId = userContext.UserId
                        };
                        await repo.AddAsync(caseSessionMeeting);

                        if (saved.SessionTypeId != NomenclatureConstants.SessionType.ClosedSession)
                        {
                            if (lawUnits != null)
                            {
                                var caseLawUnits = lawUnits.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary).ToList() ?? new List<CaseLawUnit>();
                                if (caseLawUnits.Count == 1)
                                {
                                    var caseSessionMeetingUser = new CaseSessionMeetingUser()
                                    {
                                        CourtId = saved.CourtId,
                                        CaseId = saved.CaseId,
                                        CaseSessionMeetingId = caseSessionMeeting.Id,
                                        SecretaryUserId = caseLawUnits.FirstOrDefault().LawUnitUserId,
                                        DateWrt = dateTimeNow,
                                        UserId = userContext.UserId
                                    };

                                    if (caseSessionMeetingUser.SecretaryUserId != null)
                                        caseSessionMeeting.CaseSessionMeetingUsers.Add(caseSessionMeetingUser);
                                }
                            }
                        }

                        if (model.ActTypeId != null)
                        {
                            var caseSessionAct = new CaseSessionAct()
                            {
                                CourtId = saved.CourtId,
                                CaseId = saved.CaseId,
                                CaseSessionId = saved.Id,
                                ActTypeId = model.ActTypeId ?? 0,
                                ActKindId = model.ActKindId,
                                RelatedActId = model.RelatedActId,
                                ActStateId = NomenclatureConstants.SessionActState.Project,
                                ActComplainResultId = model.ActComplainResultId,
                                IsFinalDoc = model.IsFinalDoc ?? false,
                                TDActForRegistration = model.TDActForRegistration ?? false,
                                ActISPNReasonId = model.ActISPNReasonId,
                                RnflEffectiveImmediately = model.RnflEffectiveImmediately,
                                IsReadyForPublish = false,
                                CanAppeal = model.ActCanAppeal,
                                DateWrt = dateTimeNow,
                                UserId = userContext.UserId
                            };

                            if (model.HasCorrectedAct)
                            {
                                List<int> correctedActs = new();
                                if (model.CorrectedActsIds != null)
                                {
                                    correctedActs = model.CorrectedActsIds.Select(c => int.Parse(c)).ToList();
                                }
                                caseSessionAct.ActsToCorrect = correctedActs.Select(c => new CaseSessionActCorrection
                                {
                                    CorrectedActId = c
                                }).ToList();
                            }

                            await repo.AddAsync<CaseSessionAct>(caseSessionAct);
                            await repo.SaveChangesAsync();

                            model.ActSaveId = caseSessionAct.Id;

                            if ((model.SessionResultId > 0) && (date_session <= date_now))
                            {
                                var caseSessionResult = new CaseSessionResult()
                                {
                                    CourtId = saved.CourtId,
                                    CaseId = saved.CaseId,
                                    CaseSessionId = saved.Id,
                                    SessionResultId = model.SessionResultId ?? 0,
                                    SessionResultBaseId = model.SessionResultBaseId,
                                    Description = model.Description,
                                    IsActive = true,
                                    IsMain = model.IsMainResult,
                                };

                                await repo.AddAsync(caseSessionResult);
                                AutoChangeCaseStatsFromSessionResult(model.SessionResultId ?? 0, saved.CaseId, null);

                                if (model.SessionResultId == NomenclatureConstants.CaseSessionResult.ReferralInformationalMeetingMediation)
                                {
                                    Case @case = await repo.All<Case>()
                                                           .Where(x => x.Id == saved.CaseId &&
                                                                       !(x.IsMediation ?? false))
                                                           .FirstOrDefaultAsync();

                                    if (@case != null)
                                    {
                                        @case.IsMediation = true;
                                        @case.StartMediationDate = DateTime.Now;
                                        await repo.SaveChangesAsync();
                                        var resultStart = await caseLifecycleService.StartLifecycleMediation(saved.CaseId);
                                    }
                                }

                                if ((NomenclatureConstants.CaseSessionResult.LifecycleStopOne.Contains(model.SessionResultId ?? 0) &&
                                     model.SessionResultBaseId == NomenclatureConstants.CaseSessionResultBase.AgreementReachedBetweenPartiesAfterMediation) ||
                                     (NomenclatureConstants.CaseSessionResult.LifecycleStopTwo.Contains(model.SessionResultId ?? 0) &&
                                     model.SessionResultBaseId == NomenclatureConstants.CaseSessionResultBase.WithdrawalAbandonmentClaimAfterMediationProcedure))
                                {
                                    bool resultStop = await caseLifecycleService.StopLifecycleMediation(saved.CaseId);
                                }

                                if (NomenclatureConstants.CaseSessionResult.ResultsStartNotificationN3.Contains(model.SessionResultId ?? 0))
                                {
                                    await repo.SaveChangesAsync();

                                    if (isFastProcess)
                                        await workNotificationService.SaveNotificationsForN3Result(caseSessionResult.Id);
                                }
                            }

                            if (isFastProcess)
                                await workNotificationService.SaveNotificationsForN3(caseSessionAct.Id);
                        }

                        await repo.SaveChangesAsync();

                        //Запис на пари за заседатели
                        (bool result, string errorMessage) = moneyService.CalcEarningsJury(saved, userContext.CourtId);
                        if (result == false)
                        {
                            return new SaveResultVM(false, errorMessage);
                        }

                        caseDeadlineService.DeadLineOnSession(saved);
                        await repo.SaveChangesAsync();
                        mqEpepService.AppendCaseSession(saved, EpepConstants.ServiceMethod.Add);

                        completeAllTasks_ForSession(saved.CaseId);
                        //a.stoynov
                        if (vksNotificationService != null)
                        {
                            if (userContext.CourtTypeId == NomenclatureConstants.CourtType.VKS)
                            {
                                if (vksNotificationService.IsCaseForCountryPaper(model.CaseId))
                                {
                                    var vksNotificationList = vksNotificationService.GetNotificationItem(saved.Id);
                                    vksNotificationService.SaveData(vksNotificationList);
                                }
                            }
                        }

                        //ако има промяна на състава по делото след насрочване на дело във ВКС
                        //се актуализира председателя
                        if (hasVKSLawunitChange)
                        {
                            courtLawUnitService.CourtDepartmentUnitOrder_ActualizeForCase(model.CaseId);
                        }

                        ts.Commit();
                    }
                }

                model.Id = saved.Id;

                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_CC_SaveData(model.CaseId);

                if ((model.SessionResultId > 0) && (saved.DateFrom <= dateTimeNow))
                    caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(saved.Id);

                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Заседание Id={model.Id}");
                return new SaveResultVM(false, MessageConstant.Values.SaveFailed);
            }
        }

        /// <summary>
        /// Приключва всички задачи за Насрочване по делото
        /// </summary>
        /// <param name="caseId"></param>
        private void completeAllTasks_ForSession(int caseId)
        {
            var tasks = repo.All<WorkTask>()
                                .Where(x => x.SourceType == SourceTypeSelectVM.Case && x.SourceId == caseId)
                                .Where(x => x.TaskTypeId == WorkTaskConstants.Types.SendFor_NewSession)
                                .Where(x => x.CourtId == userContext.CourtId)
                                .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                .ToList();
            if (tasks.Any())
            {
                foreach (var item in tasks)
                {
                    item.DateCompleted = DateTime.Now;
                    item.TaskStateId = WorkTaskConstants.States.Completed;
                    //repo.Update(item);
                }
                repo.SaveChanges();
            }
        }

        /// <summary>
        /// Изчитане на заседание по ИД
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public CaseSession CaseSessionById(int caseSessionId)
        {
            return repo.AllReadonly<CaseSession>()
                       .Include(x => x.Case)
                       .Include(x => x.SessionType)
                       .Include(x => x.CourtHall)
                       .Include(x => x.SessionState)
                       .Where(x => x.Id == caseSessionId)
                       .FirstOrDefault();
        }

        /// <summary>
        /// Изчитане на заседание по ИД
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public async Task<CaseSession> CaseSessionByIdAsync(int caseSessionId)
        {
            return await repo.AllReadonly<CaseSession>()
                             .Include(x => x.Case)
                             .Include(x => x.SessionType)
                             .Include(x => x.CourtHall)
                             .Include(x => x.SessionState)
                             .Where(x => x.Id == caseSessionId)
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Изчитане на данни за резултати по заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <param name="IsViewExpired"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionResultVM> CaseSessionResult_Select(int CaseSessionId, bool IsViewExpired = false)
        {
            return repo.AllReadonly<CaseSessionResult>()
                .Include(x => x.SessionResult)
                .Include(x => x.SessionResultBase)
                .Where(x => (x.CaseSessionId == CaseSessionId) &&
                            (!IsViewExpired ? x.DateExpired == null : true))
                .Select(x => new CaseSessionResultVM()
                {
                    Id = x.Id,
                    CaseSessionId = x.CaseSessionId,
                    SessionResultLabel = x.SessionResult.Label,
                    SessionResultId = x.SessionResultId,
                    SessionResultBaseLabel = (x.SessionResultBaseId != null) ? x.SessionResultBase.Label : string.Empty,
                    IsActiveText = x.IsActive ? MessageConstant.Yes : MessageConstant.No,
                    IsMainText = x.IsMain ? MessageConstant.Yes : MessageConstant.No,
                })
                .AsQueryable();
        }

        private void AutoChangeCaseStatsFromSessionResult(int sessionResultId, int caseId, int? caseSessionId)
        {
            #region Автоматизиране на статус на дело

            var sessionResultGroupId = GetPropById<SessionResult, int?>(sessionResultId, x => x.SessionResultGroupId);


            switch (sessionResultGroupId)
            {
                //   // Спиране на дело
                case NomenclatureConstants.CaseSessionResultGroups.Stop:
                    {
                        var caseCase = repo.GetById<Case>(caseId);
                        caseCase.CaseStateId = NomenclatureConstants.CaseState.Stop;
                        caseCase.DateWrt = DateTime.Now;
                        caseCase.UserId = userContext.UserId;
                    }
                    break;
                // Прекратяване на дело
                case NomenclatureConstants.CaseSessionResultGroups.Suspended:
                    {
                        var caseCase = repo.GetById<Case>(caseId);
                        caseCase.CaseStateId = NomenclatureConstants.CaseState.Suspend;
                        caseCase.DateWrt = DateTime.Now;
                        caseCase.UserId = userContext.UserId;
                    }
                    break;
            }




            if (sessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)
            {
                if (caseSessionId != null)
                {
                    if (!repo.AllReadonly<CaseSessionAct>()
                             .Any(x => x.DateExpired == null &&
                                       x.CaseSessionId == caseSessionId &&
                                       x.ActDeclaredDate != null))
                    {
                        var caseCase = repo.GetById<Case>(caseId);
                        caseCase.CaseStateId = NomenclatureConstants.CaseState.AnnouncedForResolution;
                        caseCase.DateWrt = DateTime.Now;
                        caseCase.UserId = userContext.UserId;
                        //repo.Update(caseCase);
                    }
                }
                else
                {
                    var caseCase = repo.GetById<Case>(caseId);
                    caseCase.CaseStateId = NomenclatureConstants.CaseState.AnnouncedForResolution;
                    caseCase.DateWrt = DateTime.Now;
                    caseCase.UserId = userContext.UserId;
                    //repo.Update(caseCase);
                }
            }

            #endregion
        }

        /// <summary>
        /// Запис на резултат по заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> CaseSessionResult_SaveData(CaseSessionResultEditVM model)
        {
            try
            {
                model.SessionResultBaseId = model.SessionResultBaseId.EmptyToNull();
                var caseSession = await repo.GetByIdAsync<CaseSession>(model.CaseSessionId);
                bool isFastProcess = await repo.GetPropByIdAsync<Case, bool>(x => x.Id == caseSession.CaseId, x => x.IsFastProcess ?? false);

                var saved = (model.Id > 0) ? await repo.GetByIdAsync<CaseSessionResult>(model.Id) :
                                             new CaseSessionResult()
                                             {
                                                 CourtId = model.CourtId,
                                                 CaseId = model.CaseId,
                                                 CaseSessionId = model.CaseSessionId,
                                                 SessionResultId = model.SessionResultId,
                                                 SessionResultBaseId = model.SessionResultBaseId,
                                                 Description = model.Description,
                                                 IsActive = model.IsActive,
                                                 IsMain = model.IsMain,
                                                 CaseLawUnitSelectId = model.CaseLawUnitSelectId
                                             };

                if (model.Id > 0)
                {
                    saved.SessionResultId = model.SessionResultId;
                    saved.SessionResultBaseId = model.SessionResultBaseId;
                    saved.Description = model.Description;
                    saved.IsActive = model.IsActive;
                    saved.IsMain = model.IsMain;
                    //repo.Update(saved);
                    caseDeadlineService.DeadLineOnSessionResult(saved);
                    await repo.SaveChangesAsync();
                    if (mqEpepService.ISPN_IsISPN(null, model.CaseId ?? 0))
                    {
                        mqEpepService.ISPN_CaseSessionResult(saved.Id, ServiceMethod.Add, saved.CaseId);
                    }
                    mqEpepService.AppendCaseSession(caseSession, ServiceMethod.Update);
                    caseLoadIndexService.EditSessionResultAndRecalcCase(saved.CaseId ?? 0, saved.Id);
                }
                else
                {
                    // Това да се провери после
                    await repo.AddAsync(saved);

                    AutoChangeCaseStatsFromSessionResult(model.SessionResultId, model.CaseId ?? 0, model.CaseSessionId);

                    await repo.SaveChangesAsync();
                    model.Id = saved.Id;
                    caseDeadlineService.DeadLineOnSessionResult(saved);
                    await repo.SaveChangesAsync();
                    if (mqEpepService.ISPN_IsISPN(null, model.CaseId ?? 0))
                    {
                        mqEpepService.ISPN_CaseSessionResult(model.Id, ServiceMethod.Add, model.CaseId);
                    }
                    mqEpepService.AppendCaseSession(caseSession, ServiceMethod.Update);
                }

                if (model.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession ||
                    model.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSessionWithoutDocuments)
                    caseLifecycleService.CaseLifecycle_SaveFirst_ForCaseType(model.CaseSessionId);

                if (model.SessionResultId == NomenclatureConstants.CaseSessionResult.ReferralInformationalMeetingMediation)
                {
                    Case @case = await repo.All<Case>()
                                           .Where(x => x.Id == model.CaseId &&
                                                       !(x.IsMediation ?? false))
                                           .FirstOrDefaultAsync();

                    if (@case != null)
                    {
                        @case.IsMediation = true;
                        @case.StartMediationDate = DateTime.Now;
                        await repo.SaveChangesAsync();
                        await caseLifecycleService.StartLifecycleMediation(model.CaseId ?? 0);
                    }
                }

                if ((NomenclatureConstants.CaseSessionResult.LifecycleStopOne.Contains(model.SessionResultId) &&
                     model.SessionResultBaseId == NomenclatureConstants.CaseSessionResultBase.AgreementReachedBetweenPartiesAfterMediation) ||
                     (NomenclatureConstants.CaseSessionResult.LifecycleStopTwo.Contains(model.SessionResultId) &&
                     model.SessionResultBaseId == NomenclatureConstants.CaseSessionResultBase.WithdrawalAbandonmentClaimAfterMediationProcedure))
                    await caseLifecycleService.StopLifecycleMediation(model.CaseId ?? 0);

                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(model.CaseSessionId);

                if (isFastProcess)
                    await workNotificationService.SaveNotificationsForDecreeRecusalSelfRecusalFastProcessBySessionResultId(model.Id);

                if (model.SessionResultId == NomenclatureConstants.CaseSessionResult.ByActOnProgressCaseWithoutNotifyingParties && isFastProcess)
                    await workNotificationService.SaveNotificationsForN3Result(model.Id);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на резултат за заседание Id={model.Id}");
                return false;
            }
        }

        /// <summary>
        /// Изчитане на данни по заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public async Task<CaseSessionVM> CaseSessionVMById(int caseSessionId)
        {
            return await repo.AllReadonly<CaseSession>()
                             .Where(x => x.Id == caseSessionId)
                             .Select(x => new CaseSessionVM()
                             {
                                 Id = x.Id,
                                 DateWrt = x.DateWrt,
                                 CourtId = x.CourtId,
                                 CaseId = x.CaseId,
                                 CaseName = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                 SessionTypeLabel = x.SessionType.Label,
                                 CourtHallName = (x.CourtHallId > 0) ? x.CourtHall.Name : string.Empty,
                                 SessionStateLabel = x.SessionState.Label,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                                 Description = x.Description,
                                 CourtHallId = x.CourtHallId,
                                 SessionStateId = x.SessionStateId,
                                 SessionTypeId = x.SessionTypeId,
                                 VideoUrl = x.VideoUrl,
                                 IsExpired = x.DateExpired != null,
                                 CaseTypeId = x.Case.CaseTypeId,
                                 VksLawunitChange = x.CaseLawunitChange ?? NomenclatureConstants.VksSessionLawunitChange.NoChange
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Изчитане на данни по заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public async Task<CaseSessionVM> CaseSessionVMByIdAsync(int caseSessionId)
        {
            return await repo.AllReadonly<CaseSession>()
                             .Where(x => x.Id == caseSessionId)
                             .Select(x => new CaseSessionVM()
                             {
                                 Id = x.Id,
                                 DateWrt = x.DateWrt,
                                 CourtId = x.CourtId,
                                 CaseId = x.CaseId,
                                 CaseName = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                 SessionTypeLabel = x.SessionType.Label,
                                 CourtHallName = (x.CourtHallId != null) ? x.CourtHall.Name : string.Empty,
                                 SessionStateLabel = x.SessionState.Label,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo,
                                 Description = x.Description,
                                 CourtHallId = x.CourtHallId,
                                 SessionStateId = x.SessionStateId,
                                 SessionTypeId = x.SessionTypeId,
                                 VideoUrl = x.VideoUrl,
                                 IsExpired = x.DateExpired != null,
                                 CaseTypeId = x.Case.CaseTypeId,
                                 VksLawunitChange = x.CaseLawunitChange ?? NomenclatureConstants.VksSessionLawunitChange.NoChange
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Копиране на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> CaseSession_CopyData(CaseSessionVM model)
        {
            try
            {
                var saved = (model.Id > 0) ? repo.AllReadonly<CaseSession>().Where(x => x.Id == model.Id).FirstOrDefault() : new CaseSession();

                model.CourtHallId = model.CourtHallId.EmptyToNull();
                model.CaseSessionOldId = model.Id;
                model.Id = 0;
                model.CourtId = saved.CourtId;
                model.CaseId = saved.CaseId;
                model.SessionTypeId = saved.SessionTypeId;
                model.SessionStateId = NomenclatureConstants.SessionState.Nasrocheno;
                model.DateTo = model.DateFrom.AddMinutes(model.DateTo_Minutes);
                model.CourtHallId = saved.CourtHallId;

                var result = await CaseSession_SaveData(model);

                return result.Result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при копиране на Заседание Id={model.Id}");
                return false;
            }
        }

        public async Task<SaveResultVM> CaseSession_MnogoZasedania(int caseId, int courtId)
        {
            for (int months = 4; months < 12; months++)
            {
                for (int days = 1; days < 28; days++)
                {
                    for (int hour = 8; hour < 18; hour++)
                    {
                        var model = new CaseSessionVM
                        {
                            CaseId = caseId,
                            CourtId = courtId,
                            SessionTypeId = NomenclatureConstants.SessionType.OpenSession,
                            DateFrom = new DateTime(2024, months, days, hour, 0, 0),
                            SessionStateId = NomenclatureConstants.SessionState.Nasrocheno,
                            DateTo_Minutes = 30
                        };
                        await CaseSession_SaveData(model);
                    }
                    repo.RefreshDbContext(applicationDbConnectionString);
                }
            }
            return new SaveResultVM(true);
        }

        /// <summary>
        /// Справка за заетост на зали
        /// </summary>
        /// <param name="CourtId"></param>
        /// <param name="CourtHallId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="JudgeReporterId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionHallUseVM> CaseSessionHallUse_Select(int CourtId, int? CourtHallId, DateTime? DateFrom, DateTime? DateTo, int? JudgeReporterId)
        {
            CourtHallId = CourtHallId.EmptyToNull();
            JudgeReporterId = JudgeReporterId.NumberEmptyToNull();
            var dateAddYear = DateTime.Now.AddYears(100);

            Expression<Func<CaseSessionMeeting, bool>> courtHallIdWhere = x => x.CourtHallId != null;
            if (CourtHallId != null)
                courtHallIdWhere = x => x.CourtHallId == CourtHallId;

            Expression<Func<CaseSessionMeeting, bool>> judgeReporterIdWhere = x => true;
            if (JudgeReporterId != null)
                judgeReporterIdWhere = x => x.CaseSession.CaseLawUnits.Where(a => (a.DateTo ?? dateAddYear).Date >= x.CaseSession.DateFrom && a.LawUnitId == JudgeReporterId &&
                                                                                   a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            DateFrom = DateFrom ?? DateTime.Now;
            DateTo = DateTo ?? DateTime.Now.AddDays(30);

            return repo.AllReadonly<CaseSessionMeeting>()
                       .Where(x => (x.Case.CourtId == CourtId) &&
                                   (x.CaseSession.SessionStateId != NomenclatureConstants.SessionState.Prenasrocheno && x.CaseSession.SessionStateId != NomenclatureConstants.SessionState.Cancel) &&
                                   (x.DateExpired == null) &&
                                   (x.CaseSession.DateExpired == null) &&
                                   (x.DateFrom >= DateFrom) &&
                                   (x.DateFrom <= DateTo))
                       .Where(courtHallIdWhere)
                       .Where(judgeReporterIdWhere)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .OrderBy(x => x.CourtHall)
                       .ThenBy(x => x.Id)
                       .ThenBy(x => x.DateFrom)
                       .Select(x => new CaseSessionHallUseVM()
                       {
                           Id = x.Id,
                           CaseId = x.CaseId ?? 0,
                           CaseSessionId = x.CaseSessionId,
                           SessionLabel = x.SessionMeetingType.Label +
                                          " " + x.CaseSession.SessionType.Label + " " + x.CaseSession.DateFrom.ToString("dd.MM.yyyy"),
                           SessionDate = x.CaseSession.DateFrom,
                           CaseName = x.Case.CaseType.Code + " " + x.Case.ShortNumber + "/" + x.Case.RegDate.ToString("yyyy"),
                           CaseDate = x.Case.RegDate,
                           CourtHallName = (x.CourtHallId != null) ? x.CourtHall.Name : string.Empty,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           CourtHallId = x.CourtHallId,
                           SessionTypeLabel = x.CaseSession.SessionType.Label,
                           CourtHallLocation = (x.CourtHallId != null) ? x.CourtHall.Location : string.Empty,
                           JudgeReport = x.CaseSession.CaseLawUnits.Where(l => l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter &&
                                                          (l.DateTo ?? DateTime.Now.AddYears(100)) >= x.CaseSession.DateFrom)
                                              .Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                              .FirstOrDefault()
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Изчитане на резултати от заседание
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionResultVM> CaseSessionResult_SelectByCaseId(int CaseId)
        {
            return repo.AllReadonly<CaseSessionResult>()
                       .Include(x => x.SessionResult)
                       .Include(x => x.SessionResultBase)
                       .Include(x => x.CaseSession)
                       .Where(x => x.CaseSession.CaseId == CaseId && x.DateExpired == null)
                       .Select(x => new CaseSessionResultVM()
                       {
                           Id = x.Id,
                           CaseSessionId = x.CaseSessionId,
                           SessionResultLabel = (x.SessionResult != null) ? x.SessionResult.Label : string.Empty,
                           SessionResultBaseLabel = (x.SessionResultBase != null) ? x.SessionResultBase.Label : string.Empty,
                           IsActiveText = x.IsActive ? MessageConstant.Yes : MessageConstant.No
                       })
                       .AsQueryable();
        }

        public async Task<bool> IsExistCaseSessionResult(int CaseSessionId)
        {
            return await repo.AllReadonly<CaseSessionResult>()
                             .AnyAsync(x => x.CaseSessionId == CaseSessionId && x.DateExpired == null);
        }

        /// <summary>
        /// Извличане на данни за резултати от заседание
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public List<SelectListItem> CaseSessionResultStringList_SelectByCaseId(int CaseId)
        {
            var caseSessionResults = CaseSessionResult_SelectByCaseId(CaseId).ToList();
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            foreach (var caseSessionResult in caseSessionResults)
            {
                var selectListItem = selectListItems.Where(x => x.Value == caseSessionResult.Id.ToString()).FirstOrDefault();

                if (selectListItem == null)
                {
                    SelectListItem item = new SelectListItem()
                    {
                        Text = caseSessionResult.SessionResultLabel + ((!string.IsNullOrEmpty(caseSessionResult.SessionResultBaseLabel)) ? " - " + caseSessionResult.SessionResultBaseLabel : string.Empty) + ";",
                        Value = caseSessionResult.CaseSessionId.ToString()
                    };
                    selectListItems.Add(item);
                }
                else
                    selectListItem.Text += caseSessionResult.SessionResultLabel + ((!string.IsNullOrEmpty(caseSessionResult.SessionResultBaseLabel)) ? " - " + caseSessionResult.SessionResultBaseLabel : string.Empty) + ";";
            }

            return selectListItems;
        }

        /// <summary>
        /// Проверка за заетост на зала
        /// </summary>
        /// <param name="CourtHallId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo_Minutes"></param>
        /// <param name="ModelId"></param>
        /// <returns></returns>
        public async Task<bool> CourtHallBusy(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int ModelId)
        {
            DateTime DateTo = DateFrom.AddMinutes(DateTo_Minutes);
            return await repo.AllReadonly<CaseSession>()
                             .AnyAsync(x => (x.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno) &&
                                       (x.CourtHallId == CourtHallId) &&
                                       ((ModelId > 0) ? x.Id != ModelId : true) &&
                                       (x.DateExpired == null) &&
                                       ((DateTo >= x.DateFrom) && (DateFrom <= x.DateTo)));
        }

        /// <summary>
        /// Справка за заседания
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionVM> CaseSessionSpr_Select(CaseSessionFilterVM model)
        {
            DateTime? dateFromSearch = model.DateFrom ?? DateTime.Now.AddYears(-100).ForceStartDate();
            DateTime? dateToSearch = model.DateTo ?? DateTime.Now.AddYears(100).ForceEndDate();
            DateTime dateEnd = DateTime.Now.AddYears(100);

            Expression<Func<CaseSession, bool>> dateSearch = x => x.DateFrom >= dateFromSearch && x.DateFrom <= dateToSearch;

            Expression<Func<CaseSession, bool>> yearWhere = x => true;
            if ((model.Year ?? 0) > 0)
                yearWhere = x => x.DateFrom.Year == model.Year;

            //Expression<Func<CaseSession, bool>> sessionTypeWhere = x => true;
            //if (model.CaseSessionTypeId > 0)
            //    sessionTypeWhere = x => x.SessionTypeId == model.CaseSessionTypeId;

            Expression<Func<CaseSession, bool>> sessionTypeWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseSessionTypeIds_text))
            {
                var listCaseSessionTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseSessionTypeIds_text))
                    listCaseSessionTypeIds = model.CaseSessionTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                sessionTypeWhere = x => listCaseSessionTypeIds.Contains(x.SessionTypeId);
            }

            Expression<Func<CaseSession, bool>> hallWhere = x => true;
            if (model.HallId > 0)
                hallWhere = x => x.CourtHallId == model.HallId;

            model.SecretaryUserId = model.SecretaryUserId.EmptyToNull().EmptyToNull("0").EmptyToNull("-1");
            Expression<Func<CaseSession, bool>> secretaryWhere = x => true;
            if (!string.IsNullOrEmpty(model.SecretaryUserId))
                secretaryWhere = x => x.CaseSessionMeetings.Where(m => m.CaseSessionMeetingUsers.Where(u => u.SecretaryUserId == model.SecretaryUserId).Any()).Any();

            Expression<Func<CaseSession, bool>> caseGroupWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
            {
                var listGroupIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseGroupIds_text))
                    listGroupIds = model.CaseGroupIds_text.Split(',').Select(Int32.Parse).ToList();
                caseGroupWhere = x => listGroupIds.Contains(x.Case.CaseGroupId);
            }

            Expression<Func<CaseSession, bool>> caseTypeWhere = x => true;
            if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
            {
                var listTypeIds = new List<int>();
                if (!string.IsNullOrEmpty(model.CaseTypeIds_text))
                    listTypeIds = model.CaseTypeIds_text.Split(',').Select(Int32.Parse).ToList();
                caseTypeWhere = x => listTypeIds.Contains(x.Case.CaseTypeId);
            }

            //Expression<Func<CaseSession, bool>> caseSessionResultWhere = x => true;
            //if (model.SessionResultId > 0)
            //{
            //    caseSessionResultWhere = x => x.CaseSessionResults.Any(res => res.SessionResultId == model.SessionResultId && res.DateExpired == null);
            //}

            Expression<Func<CaseSession, bool>> caseSessionResultWhere = x => true;
            if (!string.IsNullOrEmpty(model.SessionResultIds_text))
            {
                var listSessionResultIds = new List<int>();
                if (!string.IsNullOrEmpty(model.SessionResultIds_text))
                    listSessionResultIds = model.SessionResultIds_text.Split(',').Select(Int32.Parse).ToList();
                caseSessionResultWhere = x => x.CaseSessionResults.Any(res => listSessionResultIds.Contains(res.SessionResultId) && res.DateExpired == null);
            }

            Expression<Func<CaseSession, bool>> sessionStateWhere = x => true;
            if (model.SessionStateId > 0)
                sessionStateWhere = x => x.SessionStateId == model.SessionStateId;

            Expression<Func<CaseSession, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                caseRegnumberSearch = x => EF.Functions.ILike(x.Case.RegNumber, model.RegNumber.ToCasePaternSearch());

            Expression<Func<CaseSession, bool>> courtDepartment = x => true;
            if (model.CourtDepartmentId > 0)
                courtDepartment = x => x.CaseLawUnits.Any(a => a.CourtDepartmentId == model.CourtDepartmentId);

            Expression<Func<CaseSession, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd).Date >= x.DateFrom.Date && a.LawUnitId == model.JudgeReporterId &&
                                                                     a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            Expression<Func<CaseSession, bool>> courtDepartmentOtdelenie = x => true;
            if (model.CourtDepartmentOtdelenieId > 0)
                courtDepartmentOtdelenie = x => x.Case.OtdelenieId == model.CourtDepartmentOtdelenieId;

            return repo.AllReadonly<CaseSession>()
                       .Where(x => x.Case.CourtId == userContext.CourtId)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Where(x => x.DateExpired == null)
                       .Where(dateSearch)
                       .Where(yearWhere)
                       .Where(sessionTypeWhere)
                       .Where(sessionStateWhere)
                       .Where(hallWhere)
                       .Where(secretaryWhere)
                       .Where(caseGroupWhere)
                       .Where(caseTypeWhere)
                       .Where(caseSessionResultWhere)
                       .Where(caseRegnumberSearch)
                       .Where(courtDepartment)
                       .Where(judgeReporterSearch)
                       .Where(courtDepartmentOtdelenie)
                       .Select(x => new CaseSessionVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CaseId = x.CaseId,
                           CaseName = x.Case.RegNumber,
                           CaseNameText = x.Case.CaseType.Code + " " + x.Case.ShortNumber + "/" + x.Case.RegDate.ToString("yyyy"),
                           CaseTypeLabel = x.Case.CaseType.Code,
                           SessionTypeLabel = x.SessionType.Label,
                           CourtHallName = (x.CourtHallId != null) ? x.CourtHall.Name : string.Empty,
                           SessionStateLabel = x.SessionState.Label,
                           DateFrom = x.DateFrom,
                           DateFromTime = x.DateFrom,
                           DateFromDateString = x.DateFrom.ToString("dd.MM.yyyy"),
                           DateFromTimeString = x.DateFrom.ToString("HH:mm"),
                           DateTo = x.DateTo,
                           Description = x.Description,
                           CourtHallId = x.CourtHallId,
                           SessionStateId = x.SessionStateId,
                           SessionTypeId = x.SessionTypeId,
                           //DateTo_Minutes = Convert.ToInt32(((TimeSpan)(x.DateTo ?? x.DateFrom).Subtract(x.DateFrom)).TotalMinutes),
                           SessionResultIds = x.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label).FirstOrDefault(),
                           SessionResultLabel = string.Join(",", x.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label + (r.SessionResultBaseId != null ? " - " + r.SessionResultBase.Label : string.Empty))),
                           JudgeReporterLabel = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Select(a => a.LawUnit.FullName).FirstOrDefault(),
                           JudgeCompositionLabel = string.Join(", ", x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd) >= x.DateFrom && (a.JudgeRoleId == NomenclatureConstants.JudgeRole.Judge || a.JudgeRoleId == NomenclatureConstants.JudgeRole.ExtJudge)).Select(a => a.LawUnit.FullName + (a.CourtDepartmentId != null ? " (" + a.CourtDepartment.Label + ")" : string.Empty))),
                           DepartmentOtdelenieText = (x.Case.OtdelenieId != null && x.Case.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? x.Case.Otdelenie.Label : string.Empty) +
                                                     (x.Case.JudicalCompositionId != null ? (x.Case.OtdelenieId != null && x.Case.Otdelenie.DepartmentTypeId != NomenclatureConstants.DepartmentType.Napravlenie ? " / " + x.Case.JudicalComposition.Label : x.Case.JudicalComposition.Label) : string.Empty)
                       })
                       .AsQueryable();
        }

        public IEnumerable<CalendarVM> CaseSessionSprCalendar_Select(CaseSessionFilterVM model)
        {
            List<CalendarVM> result = new List<CalendarVM>();

            var sessions = CaseSessionSpr_Select(model).Select(x => new CalendarVM()
            {
                id = x.Id,
                title = x.SessionTypeLabel,
                start = x.DateFrom,
                end = x.DateTo,
                color = "#00c0ef",
                pop_content = x.CaseNameText,
                pop_title = "Дело",
                SourceId = x.Id
            }).ToList();

            foreach (var item in sessions)
            {
                item.url = urlHelper.Action("Preview", "CaseSession", new { id = item.SourceId });
            }
            result.AddRange(sessions);

            var holidays = repo.AllReadonly<WorkingDay>()
                               .Where(x => x.DayTypeId == CommonContants.WorkingDays.NotWorkDay)
                               .Where(x => x.Day >= model.DateFrom && x.Day <= model.DateTo)
                               .Where(x => (x.CourtId ?? userContext.CourtId) == userContext.CourtId)
                               .Select(x => new CalendarVM
                               {
                                   id = x.Id,
                                   //title = "Почивен ден",
                                   title = x.Description ?? "Почивен ден",
                                   start = x.Day,
                                   allDay = false,
                                   color = "#F5A9A9",
                                   pop_title = x.Description,
                                   url = "#hide"
                               }).ToList();

            foreach (var day in holidays)
            {
                day.end = day.start.MakeEndDate();

            }

            result.AddRange(holidays);

            return result;
        }

        /// <summary>
        /// Извлича обвиняемите по заседание
        /// </summary>
        /// <param name="caseSessionTimeBook"></param>
        private void FillLeftRightSide(CaseSessionTimeBookVM caseSessionTimeBook)
        {
            var casePersonLists = casePersonService.CasePersonFast_SelectForCasePreview(caseSessionTimeBook.CaseId, caseSessionTimeBook.Id).ToList();
            caseSessionTimeBook.LeftSide = string.Join("; ", casePersonLists.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide).OrderBy(x => x.FullName).Select(x => x.FullName));
            caseSessionTimeBook.RightSide = string.Join("; ", casePersonLists.Where(x => x.RoleKindId == NomenclatureConstants.PersonKinds.RightSide).OrderBy(x => x.FullName).Select(x => x.FullName));
            caseSessionTimeBook.Prosecutor = string.Join("; ", casePersonLists.Where(x => x.PersonRoleId == NomenclatureConstants.PersonRole.Prokuror).OrderBy(x => x.FullName).Select(x => x.FullName));
        }

        /// <summary>
        /// Извлича състава по заседание
        /// </summary>
        /// <param name="caseSessionTimeBook"></param>
        private void FillLowUnit(CaseSessionTimeBookVM caseSessionTimeBook)
        {
            var lawUnits = caseLawUnitService.CaseLawUnit_Select(caseSessionTimeBook.CaseId, caseSessionTimeBook.Id).ToList();

            var judgeRepSession = lawUnits.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && x.CaseSessionId == caseSessionTimeBook.Id).FirstOrDefault();
            caseSessionTimeBook.Rapporteur = judgeRepSession != null ? judgeRepSession.LawUnitName + ((!string.IsNullOrEmpty(judgeRepSession.DepartmentLabel)) ? " състав: " + judgeRepSession.DepartmentLabel : string.Empty) : string.Empty;

            if (judgeRepSession != null)
            {
                if (judgeRepSession.DepartmentId != null)
                {
                    var courtDepartment = repo.GetById<CourtDepartment>(judgeRepSession.DepartmentId);
                    var courtDepartmentLaws = repo.AllReadonly<CourtDepartmentLawUnit>()
                                                  .Include(x => x.LawUnit)
                                                  .Include(x => x.JudgeDepartmentRole)
                                                  .Where(x => x.CourtDepartmentId == courtDepartment.Id &&
                                                              x.DateFrom <= caseSessionTimeBook.DateSession &&
                                                              x.DateTo >= caseSessionTimeBook.DateSession)
                                                  .ToList();

                    caseSessionTimeBook.CourtComposition = courtDepartment.Label + " " + string.Join("; ", courtDepartmentLaws.OrderBy(x => x.LawUnit.FullName).Select(x => x.LawUnit.FullName + ((x.JudgeDepartmentRole != null) ? " (" + x.JudgeDepartmentRole.Label + ")" : string.Empty)));
                }
            }
        }

        /// <summary>
        /// Извлича данните за секретарите
        /// </summary>
        /// <param name="caseSessionTimeBook"></param>
        private void FillSecretary(CaseSessionTimeBookVM caseSessionTimeBook)
        {
            var caseSessionMeetings = repo.AllReadonly<CaseSessionMeeting>()
                                          .Include(x => x.CaseSessionMeetingUsers)
                                          .ThenInclude(x => x.SecretaryUser)
                                          .ThenInclude(x => x.LawUnit)
                                          .Where(x => x.CaseSessionId == caseSessionTimeBook.Id && x.DateExpired == null)
                                          .ToList();

            foreach (var caseSessionMeeting in caseSessionMeetings)
            {
                foreach (var caseSessionMeetingUser in caseSessionMeeting.CaseSessionMeetingUsers.OrderBy(x => x.SecretaryUser.LawUnit.FullName))
                    caseSessionTimeBook.Secretary += caseSessionMeetingUser.SecretaryUser.LawUnit.FullName + "; ";
            }
        }

        /// <summary>
        /// Извлича данни за резултати и актове по заседание
        /// </summary>
        /// <param name="caseSessionTimeBook"></param>
        private void FillResultAndAct(CaseSessionTimeBookVM caseSessionTimeBook)
        {
            var caseSessionResults = CaseSessionResult_Select(caseSessionTimeBook.Id).ToList();
            var caseSessionActs = repo.AllReadonly<CaseSessionAct>()
                                      .Include(x => x.ActType)
                                      .Include(x => x.ActState)
                                      .Where(x => x.CaseSessionId == caseSessionTimeBook.Id)
                                      .ToList();

            foreach (var caseSessionResult in caseSessionResults)
            {
                caseSessionTimeBook.ResultAndAct += caseSessionResult.SessionResultLabel + ((!string.IsNullOrEmpty(caseSessionResult.SessionResultBaseLabel)) ? " - " + caseSessionResult.SessionResultBaseLabel + "; " : "; ");
            }

            foreach (var caseSessionAct in caseSessionActs)
            {
                caseSessionTimeBook.ResultAndAct += caseSessionAct.RegNumber + "/" + caseSessionAct.RegDate?.ToString("dd.MM.yyyy") + " " + caseSessionAct.ActType.Label + " - " + caseSessionAct.ActState.Label + "; ";
                caseSessionTimeBook.Description += " " + caseSessionAct.Description;
            }
        }

        /// <summary>
        /// Извлича данни за срочна книга 
        /// </summary>
        /// <param name="CourtId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="CaseGroupeId"></param>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionTimeBookVM> CaseSessionTimeBook(int CourtId, DateTime DateFrom, DateTime DateTo, int CaseGroupeId, int DepartmentId)
        {
            var caseSessionTimeBooks = repo.AllReadonly<CaseSession>()
                                           .Include(x => x.Case)
                                           .ThenInclude(x => x.CaseGroup)
                                           .Include(x => x.CaseLawUnits)
                                           .Include(x => x.SessionType)
                                           .Include(x => x.CourtHall)
                                           .Include(x => x.SessionState)
                                           .Where(x => (x.Case.CourtId == CourtId) &&
                                                       ((CaseGroupeId > 0) ? x.Case.CaseGroupId == CaseGroupeId : true) &&
                                                       (DateFrom <= x.DateFrom && DateTo >= x.DateFrom) &&
                                                       (x.DateExpired == null) &&
                                                       (x.SessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PublicSession) &&
                                                       ((DepartmentId > 0) ? (x.CaseLawUnits.Any(l => l.CourtDepartmentId == DepartmentId)) : true))
                                           .Select(x => new CaseSessionTimeBookVM()
                                           {
                                               Id = x.Id,
                                               CaseId = x.CaseId,
                                               DateSession = x.DateFrom,
                                               CaseRegNumDate = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                               CaseGroupe = x.Case.CaseGroup.Code,
                                               Description = x.Description
                                           })
                                           .ToList();

            int num = 0;
            foreach (var caseSessionTimeBook in caseSessionTimeBooks.OrderBy(x => x.DateSession))
            {
                num++;
                caseSessionTimeBook.Number = num;
                FillLeftRightSide(caseSessionTimeBook);
                FillLowUnit(caseSessionTimeBook);
                FillSecretary(caseSessionTimeBook);
                FillResultAndAct(caseSessionTimeBook);
            }

            return caseSessionTimeBooks.AsQueryable();
        }

        /// <summary>
        /// Срочна книга в ексел
        /// </summary>
        /// <param name="CourtId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="CaseGroupeId"></param>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public byte[] CaseSessionTimeBook_ToExcel(int CourtId, DateTime DateFrom, DateTime DateTo, int CaseGroupeId, int DepartmentId)
        {
            NPoiExcelService excelService = new NPoiExcelService("Срочна книга");
            var caseSessionTimeBooks = CaseSessionTimeBook(CourtId, DateFrom, DateTo, CaseGroupeId, DepartmentId).OrderBy(x => x.DateSession).ToList();

            int colCnt = 10;
            excelService.AddRange("Срочна книга", colCnt, excelService.CreateTitleStyle());
            excelService.AddRow();

            excelService.AddList(
                        caseSessionTimeBooks,
                        new int[] { 4000, 4000, 4000,
                                    4000, 4000, 4000,
                                    4000, 4000, 4000,
                                    4000, 4000, 4000,
                                    4000, 4000},
                        new List<Expression<Func<CaseSessionTimeBookVM, object>>>()
                        {
                            x => x.DateSession,
                            x => x.Number,
                            x => x.CaseRegNumDate,
                            x => x.CaseGroupe,
                            x => x.LeftSide,
                            x => x.RightSide,
                            x => x.CourtComposition,
                            x => x.Rapporteur,
                            x => x.Prosecutor,
                            x => x.Secretary,
                            x => x.ResultAndAct,
                            x => x.Description,
                            x => x.DateCase,
                            x => x.Signature
                        },
                        NPOI.HSSF.Util.HSSFColor.Grey40Percent.Index,
                        NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index,
                        NPOI.HSSF.Util.HSSFColor.White.Index
                    );

            return excelService.ToArray();
        }

        /// <summary>
        /// Проверка за заетос на състав
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo_Minutes"></param>
        /// <param name="ModelId"></param>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public string IsBusyLawUnit(DateTime DateFrom, int DateTo_Minutes, int ModelId, int CaseId)
        {
            var result = string.Empty;
            var lawUnits = new List<CaseLawUnit>();

            if (ModelId < 1)
            {
                lawUnits = repo.AllReadonly<CaseLawUnit>()
                               .Include(x => x.LawUnit)
                               .Where(x => x.CaseId == CaseId &&
                                           x.CaseSessionId == null &&
                                           !NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId))
                               .ToList();
            }
            else
            {
                lawUnits = repo.AllReadonly<CaseLawUnit>()
                               .Include(x => x.LawUnit)
                               .Where(x => x.CaseId == CaseId &&
                                           x.CaseSessionId == ModelId &&
                                           !NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId))
                               .ToList();
            }


            DateTime DateTo = DateFrom.AddMinutes(DateTo_Minutes);
            var caseSessions = repo.AllReadonly<CaseSession>()
                                   .Include(x => x.CaseLawUnits)
                                   .Include(x => x.Case)
                                   .Where(x => (x.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno) &&
                                               ((ModelId > 0) ? x.Id != ModelId : true) &&
                                               (x.DateExpired == null) &&
                                               ((DateTo >= x.DateFrom) && (DateFrom <= x.DateTo)))
                                   .ToList();

            foreach (var caseSession in caseSessions)
            {
                foreach (var caseLawUnit in lawUnits)
                {
                    if (caseSession.CaseLawUnits.Any(x => x.LawUnitId == caseLawUnit.LawUnitId))
                    {
                        result += "Има застъпване с друго заседание на: " + caseLawUnit.LawUnit.FullName + " по дело: " + caseSession.Case.RegNumber + "/" + caseSession.Case.RegDate.ToString("dd.MM.yyyy") + System.Environment.NewLine;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Връща стринг с имена от подаден лист
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string GetStringPerson(ICollection<CasePerson> model)
        {
            var result = string.Empty;

            foreach (var person in model)
            {
                if (!string.IsNullOrEmpty(result))
                    result += ", ";

                result += person.FullName + (person.PersonRole != null ? " (" + person.PersonRole.Label + ")" : string.Empty) + (person.PersonMaturity != null ? " - " + person.PersonMaturity.Label : string.Empty);
            }

            return result;
        }

        /// <summary>
        /// Справка за Заседания за период с участието на малолетни/непълнолетни лица
        /// </summary>
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionSprVM> CaseSessionReportMaturity_Select(int courtId, CaseFilterReport model)
        {
            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom ?? DateTime.Now.AddYears(-100));
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo ?? DateTime.Now.AddYears(100));

            Expression<Func<CaseSession, bool>> caseGroupIdWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseSession, bool>> caseTypeIdWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<CaseSession, bool>> sessionTypeIdWhere = x => true;
            if (model.SessionTypeId > 0)
                sessionTypeIdWhere = x => x.SessionTypeId == model.SessionTypeId;

            Expression<Func<CaseSession, bool>> judgeReporterIdWhere = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseLawUnits.Where(a => (a.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.Case.RegDate.Date && a.LawUnitId == model.JudgeReporterId &&
                                                                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            return repo.AllReadonly<CaseSession>()
                       //.Include(x => x.SessionType)
                       //.Include(x => x.CasePersons)
                       //.ThenInclude(x => x.PersonMaturity)
                       //.Include(x => x.CasePersons)
                       //.ThenInclude(x => x.PersonRole)
                       //.Include(x => x.CaseLawUnits)
                       //.ThenInclude(x => x.LawUnit)
                       //.Include(x => x.CaseLawUnits)
                       //.ThenInclude(x => x.JudgeRole)
                       //.Include(x => x.CaseLawUnits)
                       //.ThenInclude(x => x.CourtDepartment)
                       //.Include(x => x.Case)
                       //.ThenInclude(x => x.CaseGroup)
                       //.Include(x => x.Case)
                       //.ThenInclude(x => x.CaseType)
                       //.Include(x => x.Case)
                       //.ThenInclude(x => x.CaseCode)
                       //.Include(x => x.CaseSessionResults)
                       //.ThenInclude(x => x.SessionResult)
                       .Where(x => (x.CourtId == courtId) && x.DateExpired == null &&
                                   ((x.DateFrom >= model.DateFrom) && (x.DateFrom <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                   (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   (x.CasePersons.Any(p => p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge)))
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(sessionTypeIdWhere)
                       .Where(judgeReporterIdWhere)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new CaseSessionSprVM()
                       {
                           CaseGroupLabel = x.Case.CaseGroup.Label + " - " + x.Case.CaseType.Label,
                           CaseRegNum = x.Case.RegNumber,
                           CaseRegDate = x.Case.RegDate,
                           JudgeReport = x.CaseLawUnits.Where(l => l.CaseSessionId == x.Id &&
                                                                   (l.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.Case.RegDate.Date &&
                                                                   l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                       .Select(l => l.LawUnit.FullName + ((l.CourtDepartmentId != null) ? " състав: " + l.CourtDepartment.Label : string.Empty))
                                                       .FirstOrDefault(),
                           SessionTypeLabel = x.SessionType.Label,
                           SessionDateFrom = x.DateFrom,
                           CasePersons = string.Join(", ", x.CasePersons
                                                            .Where(p => p.CaseSessionId == x.Id &&
                                                                        (p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged ||
                                                                         p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge))
                                                            .Select(p => p.FullName +
                                                                         " (" + p.PersonRole.Label + ")" +
                                                                         (p.PersonMaturityId != null ? " - " + p.PersonMaturity.Label : string.Empty))),
                           CasePerson = x.CasePersons
                                         .Where(p => p.CaseSessionId == x.Id &&
                                                     (p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged ||
                                                      p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge))
                                         .Select(p => p.FullName)
                                         .FirstOrDefault(),
                           ActDeclaredDate = x.CaseSessionActs.Where(a => a.IsFinalDoc).Select(a => a.ActDeclaredDate).FirstOrDefault(),
                           SessionResultLabel = x.CaseSessionResults.Where(r => r.IsMain && r.DateExpired == null).Select(r => r.SessionResult.Label).FirstOrDefault(),
                           Id = x.Id,
                           CaseId = x.CaseId,
                           SessionDateReturn = x.CaseSessionActs.Where(a => a.IsFinalDoc).Select(a => a.RegDate).FirstOrDefault(),
                           SessionDateEntryIntoForce = x.CaseSessionActs.Where(a => a.IsFinalDoc).Select(a => a.ActInforcedDate).FirstOrDefault()
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Проверка дали съществува основен резултат
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="SelectResultId"></param>
        /// <returns></returns>
        public bool IsExistMainResult(int caseSessionId, int SelectResultId = 0)
        {
            return repo.AllReadonly<CaseSessionResult>()
                       .Any(x => (x.CaseSessionId == caseSessionId) &&
                                 (SelectResultId > 0 ? x.Id != SelectResultId : true) &&
                                 (x.IsMain) &&
                                 (x.DateExpired == null));
        }

        /// <summary>
        /// Връща списък на заседания по дело за комбо бокс
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_CaseSessionForCopy(int CaseSessionId)
        {
            var caseSession = repo.GetById<CaseSession>(CaseSessionId);
            var result = repo.AllReadonly<CaseSession>()
                             .Include(x => x.SessionType)
                             .Where(x => x.CaseId == caseSession.CaseId && x.DateFrom < caseSession.DateFrom && x.DateExpired == null)
                             .OrderByDescending(x => x.DateFrom)
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.SessionType.Label + " от " + x.DateFrom.ToString("dd.MM.yyyy HH:mm")
                             }).ToList();
            result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();

            return result;
        }

        public List<SelectListItem> GetDropDownList_CaseSessionByCase(int CaseId, DateTime? DateFrom)
        {
            var result = repo.AllReadonly<CaseSession>()
                             .Include(x => x.SessionType)
                             .Where(x => x.CaseId == CaseId &&
                                         (DateFrom != null ? x.DateFrom >= DateFrom : true) &&
                                         x.DateExpired == null)
                             .OrderByDescending(x => x.DateFrom)
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.SessionType.Label + " от " + x.DateFrom.ToString("dd.MM.yyyy HH:mm")
                             }).ToList();

            result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();

            return result;
        }

        /// <summary>
        /// Справка за Заседания с не написани съдебни актове от всички съдии
        /// </summary>
        /// <param name="filter">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseSessionWithActProject_Select(CaseFilterReport filter)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            var dateNow = DateTime.Now;

            filter.DateFrom = NomenclatureExtensions.ForceStartDate(filter.DateFrom ?? DateTime.Now.AddYears(-100));
            filter.DateTo = DateTime.Now.AddYears(100);
            filter.ActDateToSpr = filter.ActDateToSpr == null ? NomenclatureExtensions.ForceEndDate(DateTime.Now) : NomenclatureExtensions.ForceEndDate(filter.ActDateToSpr);

            Expression<Func<CaseSession, bool>> caseGroupIdWhere = x => true;
            if (filter.CaseGroupId > 0)
                caseGroupIdWhere = x => x.Case.CaseGroupId == filter.CaseGroupId;

            Expression<Func<CaseSession, bool>> caseTypeIdWhere = x => true;
            if (filter.CaseTypeId > 0)
                caseTypeIdWhere = x => x.Case.CaseTypeId == filter.CaseTypeId;

            Expression<Func<CaseSession, bool>> caseCodeIdWhere = x => true;
            if (filter.CaseCodeId > 0)
                caseCodeIdWhere = x => x.Case.CaseCodeId == filter.CaseCodeId;

            Expression<Func<CaseSession, bool>> caseCodeIdsWhere = x => true;
            if (filter.CaseCodeIds != null && filter.CaseCodeIds.Any())
            {
                int[] caseCodeIds = filter.CaseCodeIds.Select(x => int.Parse(x)).ToArray();
                caseCodeIdsWhere = x => caseCodeIds.Contains(x.Case.CaseCodeId ?? 0);
            }

            Expression<Func<CaseSession, bool>> sessionTypeIdWhere = x => true;
            if (filter.SessionTypeIds != null && filter.SessionTypeIds.Any())
            {
                int[] sessionTypeIds = filter.SessionTypeIds.Select(x => int.Parse(x)).ToArray();
                sessionTypeIdWhere = x => sessionTypeIds.Contains(x.SessionTypeId);
            }

            Expression<Func<CaseSession, bool>> judgeReporterIdWhere = x => true;
            if (filter.JudgeReporterId > 0)
                judgeReporterIdWhere = x => x.CaseLawUnits.Where(a => (a.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.DateFrom && a.LawUnitId == filter.JudgeReporterId &&
                                                                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            return readonlyrepo.AllReadonly<CaseSession>()
                       .Where(x => (x.CourtId == userContext.CourtId) && x.DateExpired == null &&
                                   ((x.DateFrom >= filter.DateFrom) && (x.DateTo <= (filter.ActDateToSpr ?? DateTime.Now.AddYears(100)))) &&
                                   (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                   (x.SessionStateId == NomenclatureConstants.SessionState.Provedeno) &&
                                   (!x.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                                b.ActDeclaredDate != null &&
                                                                b.ActDeclaredDate <= filter.ActDateToSpr &&
                                                                (b.ActTypeId != NomenclatureConstants.ActType.Protokol ||
                                                                 (b.ActTypeId == NomenclatureConstants.ActType.Protokol &&
                                                                  x.CaseSessionResults.Any(r => r.DateExpired == null &&
                                                                                                (r.SessionResult.SessionResultGroupId != null ||
                                                                                                 NomenclatureConstants.CaseSessionResult.CaseSessionWithActProject.Contains(r.SessionResultId)))))) ||
                                   (x.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                               b.IsFinalDoc &&
                                                               b.ActDeclaredDate != null &&
                                                               b.ActDeclaredDate <= filter.ActDateToSpr &&
                                                               b.ActTypeId == NomenclatureConstants.ActType.Sentence &&
                                                               b.ActMotivesDeclaredDate == null) &&
                                    x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo) ||
                                   (x.CaseSessionActs.Any(b => b.DateExpired == null &&
                                                               b.IsFinalDoc &&
                                                               b.ActDeclaredDate != null &&
                                                               b.ActDeclaredDate <= filter.ActDateToSpr &&
                                                               b.ActTypeId == NomenclatureConstants.ActType.Answer &&
                                                               (b.ActMotivesDeclaredDate == null || b.ActMotivesDeclaredDate > filter.ActDateToSpr)) &&
                                    (!x.CaseSessionResults.Any(r => r.DateExpired == null && r.SessionResultId == NomenclatureConstants.CaseSessionResult.WithDecisionAndMotives) ||
                                     !x.CaseSessionResults.Any(r => r.DateExpired == null)) &&
                                    (x.Case.CaseGroupId == NomenclatureConstants.CaseGroups.NakazatelnoDelo))))
                       .Where(caseGroupIdWhere)
                       .Where(caseTypeIdWhere)
                       .Where(caseCodeIdWhere)
                       .Where(sessionTypeIdWhere)
                       .Where(judgeReporterIdWhere)
                       .Where(caseCodeIdsWhere)
                       .Where(x => !x.Case.CaseDeactivations.Any(d => d.CaseId == x.CaseId && d.DateExpired == null))
                       .Select(x => new CaseSprVM()
                       {
                           Id = x.CaseId,
                           JudgeReport = x.CaseLawUnits.Where(c => (c.DateTo ?? dateNow.AddYears(100)).Date >= x.DateFrom && c.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                       .Select(c => c.LawUnit.FullName + ((c.CourtDepartment != null) ? " състав: " + c.CourtDepartment.Label : String.Empty))
                                                       .FirstOrDefault(),
                           CaseTypeLabel = x.Case.CaseType.Label,
                           CaseRegNum = x.Case.RegNumber,
                           CaseCodeLabel = x.Case.CaseCode.Code,
                           SessionTypeLabel = x.SessionType.Label,
                           SessionDateFrom = x.DateFrom,
                           SessionResult = string.Join(", ", x.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label)),
                           SessionActDate = x.CaseSessionActs.Any(a => a.ActStateId == NomenclatureConstants.SessionActState.Project && a.DateExpired == null) ? x.CaseSessionActs.Where(a => a.ActStateId == NomenclatureConstants.SessionActState.Project && a.DateExpired == null).Select(a => a.RegDate).FirstOrDefault() :
                                                                                                                                                                 (x.CaseSessionActs.Any(a => a.ActDeclaredDate <= filter.ActDateToSpr && a.DateExpired == null) ? x.CaseSessionActs.Where(a => a.ActDeclaredDate <= filter.ActDateToSpr && a.DateExpired == null).Select(a => a.RegDate).FirstOrDefault() :
                                                                                                                                                                                                                                                                 (DateTime?)null)
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Проверка дали заседанието може да бъде сторнирано
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public bool IsCanExpired(int CaseSessionId)
        {


            if (repo.AllReadonly<CaseSessionAct>()
                    .Any(x => x.CaseSessionId == CaseSessionId &&
                              x.DateExpired == null))
            {
                return false;
            }

            if (repo.AllReadonly<CaseNotification>()
                    .Any(x => x.CaseSessionId == CaseSessionId &&
                              x.DateExpired == null))
            {
                return false;
            }

            if (repo.AllReadonly<CaseSessionFastDocument>()
                    .Any(x => x.CaseSessionId == CaseSessionId &&
                              x.DateExpired == null))
            {
                return false;
            }

            if (repo.AllReadonly<CaseSessionDoc>()
                    .Any(x => x.CaseSessionId == CaseSessionId &&
                              x.DateExpired == null))
            {
                return false;
            }

            var obligations = moneyService.Obligation_Select(0, 0, CaseSessionId, userContext.CourtId, 0);
            if (obligations.Any(x => x.ExpenseOrderId > 0))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка по ид дали заседанието е последното проведено
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public bool IsLastConductedSession(int CaseSessionId)
        {
            var caseSession = repo.GetById<CaseSession>(CaseSessionId);
            return !repo.AllReadonly<CaseSession>()
                        .Any(x => x.CaseId == caseSession.CaseId &&
                                  x.DateExpired == null &&
                                  x.DateFrom > caseSession.DateFrom &&
                                  x.SessionStateId == NomenclatureConstants.SessionState.Provedeno);
        }

        public IEnumerable<CalendarVM> CaseSessionHallUseCalendar_Select(int CourtId, int? CourtHallId, DateTime? DateFrom, DateTime? DateТо)
        {
            List<CalendarVM> result = new List<CalendarVM>();

            var sessions = CaseSessionHallUse_Select(CourtId, CourtHallId, DateFrom, DateТо, null).Select(x => new CalendarVM()
            {
                id = x.Id,
                title = x.SessionTypeLabel,
                start = x.DateFrom,
                end = x.DateTo,
                color = "#00c0ef",
                pop_content = x.CaseName,
                pop_title = "Дело",
                SourceId = x.CaseSessionId
            }).ToList();

            foreach (var item in sessions)
            {
                item.url = urlHelper.Action("Preview", "CaseSession", new { id = item.SourceId });
            }
            result.AddRange(sessions);

            var holidays = repo.AllReadonly<WorkingDay>()
                               .Where(x => x.DayTypeId == CommonContants.WorkingDays.NotWorkDay)
                               .Where(x => x.Day >= DateFrom && x.Day <= DateТо)
                               .Where(x => (x.CourtId ?? userContext.CourtId) == userContext.CourtId)
                               .Select(x => new CalendarVM
                               {
                                   id = x.Id,
                                   //title = "Почивен ден",
                                   title = x.Description ?? "Почивен ден",
                                   start = x.Day,
                                   allDay = false,
                                   color = "#F5A9A9",
                                   pop_title = x.Description,
                                   url = "#hide"
                               }).ToList();

            foreach (var day in holidays)
            {
                day.end = day.start.MakeEndDate();

            }

            result.AddRange(holidays);

            return result;
        }

        public byte[] ListDataSprExportExcel(CaseSessionFilterVM model)
        {
            NPoiExcelService excelService = new NPoiExcelService("Sheet1");
            var dataRows = CaseSessionSpr_Select(model).ToList();
            var num = 0;
            foreach (var caseSessionVM in dataRows.OrderBy(x => x.DateFrom))
            {
                num++;
                caseSessionVM.NumberRow = num;
            }

            string dateFrom = model.DateFrom != null ? ((DateTime)model.DateFrom).ToString("dd.MM.yyyy") : "";
            string dateTo = model.DateTo != null ? ((DateTime)model.DateTo).ToString("dd.MM.yyyy") : "";

            var otdelenie = (model.CourtDepartmentOtdelenieId > 0) ? repo.GetById<CourtDepartment>(model.CourtDepartmentOtdelenieId) : null;
            var court = repo.GetById<Court>(userContext.CourtId);

            var labelSpr = "СПИСЪК" + System.Environment.NewLine +
                           "на делата на" + ((otdelenie != null) ? " " + otdelenie.Label + " отделение," : string.Empty) + ((court != null) ? " " + court.Label + "," : string.Empty) + System.Environment.NewLine +
                           "които ще се разглеждат на" + (model.DateFrom != null ? " " + (model.DateFrom ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) + (model.DateTo != null ? " - " + (model.DateTo ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty) + (model.Year != null ? " " + model.Year : string.Empty) + System.Environment.NewLine;


            excelService.AddRange(labelSpr, 12, excelService.CreateTitleStyle());
            excelService.AddRow();
            excelService.AddRow();
            excelService.AddRow();
            excelService.AddRow();

            excelService.AddList(dataRows.OrderBy(x => x.NumberRow).ToList(),
                                 new int[] { 2000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                                 new List<Expression<Func<CaseSessionVM, object>>>()
                                 {
                                     x => x.NumberRow,
                                     x => x.CaseTypeLabel,
                                     x => x.CaseName,
                                     x => x.DateFromDateString,
                                     x => x.DateFromTimeString,
                                     x => x.CourtHallName,
                                     x => x.JudgeReporterLabel,
                                     x => x.DepartmentOtdelenieText,
                                     x => x.JudgeCompositionLabel,
                                     x => x.SessionTypeLabel,
                                     x => x.SessionStateLabel,
                                     x => x.SessionResultLabel,
                                 },
                                 NPOI.HSSF.Util.HSSFColor.White.Index,
                                 NPOI.HSSF.Util.HSSFColor.White.Index,
                                 NPOI.HSSF.Util.HSSFColor.White.Index);

            excelService.AddRow();
            excelService.AddRow();
            excelService.AddRange(dataRows.Count + " бр. записа отговарят на зададените критерии", 9);

            return excelService.ToArray();
        }

        public bool CaseSessionResult_ExpiredInfo(ExpiredInfoVM model)
        {
            if (!SaveExpireInfo<CaseSessionResult>(model))
                return false;
            var sessionResult = repo.AllReadonly<CaseSessionResult>()
                                    .Where(x => x.Id == model.Id)
                                    .FirstOrDefault();
            if (sessionResult != null)
            {
                caseDeadlineService.DeadLineOnSessionResult(sessionResult);
                caseLoadIndexService.EditSessionResultAndRecalcCase(sessionResult.CaseId ?? 0, sessionResult.Id);
                repo.SaveChanges();
            }

            return true;
        }
        public bool CaseSession_ExpiredInfo(ExpiredInfoVM model)
        {
            try
            {
                using (var ts = repo.BeginTransaction())
                {
                    if (!SaveExpireInfo<CaseSession>(model))
                        return false;
                    var session = repo.AllReadonly<CaseSession>()
                                            .Where(x => x.Id == model.Id)
                                            .FirstOrDefault();
                    if (session != null)
                    {

                        caseDeadlineService.DeadLineOnSession(session);
                        repo.SaveChanges();

                        updateVksLawunitsAfterExpire(session);

                        mqEpepService.AppendCaseSession(session, ServiceMethod.Delete);

                        (bool result, string errorMessage) = moneyService.CalcEarningsJury(session, userContext.CourtId);
                        if (result == false)
                        {
                            return false;
                        }

                        ts.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при премахване на Заседание Id={model.Id}");
                return false;
            }

            return true;
        }

        private void updateVksLawunitsAfterExpire(CaseSession model)
        {
            if (userContext.CourtTypeId != NomenclatureConstants.CourtType.VKS)
            {
                return;
            }

            var lawunitsToDelete = repo.All<CaseLawUnit>()
                                    .Where(x => x.CaseId == model.CaseId && x.FromCaseSessionId == model.Id)
                                    .ToList();

            var lawunitsToUpdate = repo.All<CaseLawUnit>()
                                    .Where(x => x.CaseId == model.CaseId && x.ToCaseSessionId == model.Id)
                                    .ToList();
            foreach (var item in lawunitsToUpdate)
            {
                item.DateTo = null;
                item.ToCaseSessionId = null;
            }

            repo.DeleteRange(lawunitsToDelete);

            if (lawunitsToDelete.Any() || lawunitsToUpdate.Any())
            {
                repo.SaveChanges();
            }
        }

        public bool IsExistCaseSession(int CaseId)
        {
            return repo.AllReadonly<CaseSession>()
                       .Any(x => x.CaseId == CaseId &&
                                 x.DateExpired == null);
        }

        public List<CheckListVM> GetCheckListCaseSession(int caseId)
        {
            var dateTimeTo = DateTime.Now;
            return repo.AllReadonly<CaseSession>()
                       .Where(x => x.CaseId == caseId &&
                                   x.DateExpired == null &&
                                   x.DateTo <= dateTimeTo &&
                                   x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession &&
                                   !x.CaseSessionActs.Any(a => a.DateExpired == null))
                       .Select(x => new CheckListVM
                       {
                           Checked = false,
                           Label = x.SessionType.Label + " от: " + x.DateFrom.ToString("dd.MM.yyyy HH:mm"),
                           Value = x.Id.ToString()
                       })
                       .ToList();
        }


        public async Task<List<SelectListItem>> GetDDL_CaseSessionAddAct(int caseId)
        {
            var dateTimeTo = DateTime.Now;
            var result = await repo.AllReadonly<CaseSession>()
                                   .Where(x => x.CaseId == caseId &&
                                               x.DateExpired == null &&
                                               x.DateTo <= dateTimeTo &&
                                               x.SessionTypeId == NomenclatureConstants.SessionType.ClosedSession &&
                                               !x.CaseSessionActs.Any(a => a.DateExpired == null))
                                   .Select(x => new SelectListItem
                                   {
                                       Value = x.Id.ToString(),
                                       Text = x.SessionType.Label + " от " + x.DateFrom.ToString("dd.MM.yyyy HH:mm")
                                   })
                                   .ToListAsync();

            result = result
                    .Prepend(new SelectListItem() { Text = "Избери", Value = "0" })
                    .ToList();

            return result;
        }

        public CaseSessionResultEditVM GetSessionResultEditVMById(int Id)
        {
            return repo.AllReadonly<CaseSessionResult>()
                       .Where(x => x.Id == Id)
                       .Select(x => new CaseSessionResultEditVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CaseId = x.CaseId,
                           CaseSessionId = x.CaseSessionId,
                           SessionResultId = x.SessionResultId,
                           SessionResultBaseId = x.SessionResultBaseId,
                           Description = x.Description,
                           IsMain = x.IsMain,
                           IsActive = x.IsActive
                       })
                       .FirstOrDefault();
        }

        public async Task<CaseSessionResultEditVM> GetSessionResultEditVMByIdAsync(int Id)
        {
            return await repo.AllReadonly<CaseSessionResult>()
                             .Where(x => x.Id == Id)
                             .Select(x => new CaseSessionResultEditVM()
                             {
                                 Id = x.Id,
                                 CourtId = x.CourtId,
                                 CaseId = x.CaseId,
                                 CaseSessionId = x.CaseSessionId,
                                 SessionResultId = x.SessionResultId,
                                 SessionResultBaseId = x.SessionResultBaseId,
                                 Description = x.Description,
                                 IsMain = x.IsMain,
                                 IsActive = x.IsActive
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        ///ВКС!!! Актуализира състава на делото в зависимост от избраната дата на заседанието за дела на ВКС  
        /// </summary>
        private bool updateCaseLawUnitByDateVKS(CaseSession model)
        {
            if (userContext.CourtTypeId != NomenclatureConstants.CourtType.VKS)
            {
                return false;
            }

            var caseModel = repo.GetById<Case>(model.CaseId);
            if (caseModel.CaseGroupId != NomenclatureConstants.CaseGroups.NakazatelnoDelo)
            {
                return false;
            }

            //Ако точния вид дело не се насрочва през календара на НД - нищо не се променя
            var excluded_case_types = SystemParam_SelectIntValues(NomenclatureConstants.SystemParamName.VKS_CaseType_CalendarExclude);
            if (excluded_case_types.Contains(caseModel.CaseTypeId))
            {
                return false;
            }

            //Всички съдии по делото
            var caseJudges = repo.All<CaseLawUnit>()
                                    .Where(x => x.CaseId == model.CaseId && x.CaseSessionId == null)
                                    .Where(x => (x.DateTo ?? DateTime.MaxValue) >= model.DateFrom)
                                    .ToList();

            var hasAnyFromSession = caseJudges.Any(x => x.FromCaseSessionId > 0);

            //Актуалния съдия-докладчик
            var caseReporter = caseJudges
                                    .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                    .FirstOrDefault();



            //Тегли състава за това отделение за дадената дата
            var vksJudgesByDate = vksSelectJudgesByReporterDate(caseReporter.LawUnitId, model.DateFrom);
            if (vksJudgesByDate == null || !vksJudgesByDate.Judges.Any())
            {
                return false;
            }

            //Ако текущо няма избрано отделение се взема от графика при първо насрочване
            if (caseModel.OtdelenieId == null || caseReporter.CourtDepartmentId == null)
            {
                caseReporter.RealCourtDepartmentId = vksJudgesByDate.CourtDepartmentId;
                caseReporter.CourtDepartmentId = vksJudgesByDate.CourtDepartmentId;
                caseModel.OtdelenieId = vksJudgesByDate.CourtDepartmentId;
            }

            //Ако има съдии, добавени по график на заседание на ВКС
            //Всички се преустановяват до датата на провеждане на 
            if (hasAnyFromSession)
            {
                foreach (var judge in caseJudges)
                {
                    if (judge.FromCaseSessionId > 0)
                    {
                        //10 минути преди началото на новото заседанието
                        judge.DateTo = model.DateFrom.AddMinutes(-10);
                        judge.ToCaseSessionId = model.Id;
                        repo.Update(judge);
                    }
                }
            }

            //Добавя всички съдии от състава без съдията докладчик по делото
            //Съдията докладчик е вече избран с протокол и наличен там
            var newJudgesDateFrom = model.DateFrom;
            if (!hasAnyFromSession)
            {
                //Ако няма преддходни съдии, добавени през график началната дата на съдиите е датата на насрочване - днес
                newJudgesDateFrom = DateTime.Now;
            }
            foreach (var item in vksJudgesByDate.Judges)
            {
                var newJudge = new CaseLawUnit()
                {
                    CaseId = model.CaseId,
                    CaseSessionId = null,
                    CourtId = caseReporter.CourtId,
                    CourtDepartmentId = caseReporter.CourtDepartmentId,
                    JudgeRoleId = NomenclatureConstants.JudgeRole.Judge,
                    JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Member,
                    DateFrom = newJudgesDateFrom,
                    DateTo = null,
                    LawUnitId = item.LawunitId,
                    LawUnitUserId = item.LawunitUserId,
                    CaseSelectionProtokolId = caseReporter.CaseSelectionProtokolId,
                    FromCaseSessionId = model.Id
                };
                repo.Add(newJudge);

            }
            if (hasAnyFromSession || vksJudgesByDate.Judges.Any())
            {
                repo.SaveChanges();

                return true;

            }

            return false;
        }

        /// <summary>
        /// ВКС!!! Изтегля списък на всички съдии за избрана дата на заседание по отделение
        /// </summary>
        /// <param name="courtDepartmentId"></param>
        /// <param name="sessionDate"></param>
        /// <returns></returns>
        private List<(int lawunitId, string lawunitUserId)> vksSelectJudgesByDate(int courtDepartmentId, DateTime sessionDate)
        {
            var selection = repo.AllReadonly<VksSelectionMonth>()
                                       .Include(x => x.SelectionMonthLawunit)
                                       .Include(x => x.VksSelection)
                                       .ThenInclude(x => x.SelectionLawunit)
                                       .Where(x => x.SessionDate >= sessionDate.Date
                                           && x.SessionDate <= sessionDate.ForceEndDate())
                                       .Where(x => x.VksSelection.CourtDepartmentId == courtDepartmentId)
                                       .FirstOrDefault();

            if (selection == null)
            {
                return null;
            }

            var result = new List<(int lawunitId, string lawunitUserId)>();

            foreach (var item in selection.SelectionMonthLawunit)
            {
                var sUnit = selection.VksSelection.SelectionLawunit
                                        .Where(s => s.Id == item.VksSelectionLawunitId && s.LawunitId > 0)
                                        .Where(s => s.DateStart <= sessionDate && (s.DateEnd ?? DateTime.MaxValue) > sessionDate)
                                        .FirstOrDefault();
                if (sUnit == null)
                {
                    continue;
                }

                (int lawunitId, string lawunitUserId) newItem = (sUnit.LawunitId.Value, commonService.Users_GetUserIdByLawunit(sUnit.LawunitId.Value));

                result.Add(newItem);
            }

            return result;
        }


        /// <summary>
        /// ВСК!!! Изтегля списък на останали съдии от графика, в който участва СД на подадената дата
        /// </summary>
        /// <param name="judgeReporterId"></param>
        /// <param name="sessionDate"></param>
        /// <returns></returns>
        private VksSessionDateJudgesVM vksSelectJudgesByReporterDate(int judgeReporterId, DateTime sessionDate)
        {
            var selection = repo.AllReadonly<VksSelectionMonth>()
                                       .Include(x => x.SelectionMonthLawunit)
                                       .ThenInclude(x => x.VksSelectionLawunit)
                                       .Include(x => x.VksSelection)
                                       .Where(x => x.SessionDate >= sessionDate.Date
                                           && x.SessionDate <= sessionDate.ForceEndDate())
                                       .Where(x => x.SelectionMonthLawunit.Any(ml => ml.VksSelectionLawunit.LawunitId == judgeReporterId))
                                       .Select(x => new
                                       {
                                           CourtDepartmentId = x.VksSelection.CourtDepartmentId,
                                           IsPredsedatel = x.VksSelection.SelectionLawunit
                                                        .Where(l => l.LawunitId == judgeReporterId)
                                                        .Where(l => l.CourtDepartmentTypeId == NomenclatureConstants.DepartmentType.Kolegia)
                                                        .Where(l => l.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                                                        .Any(),
                                           OtherJudges = x.SelectionMonthLawunit
                                                            .Where(ml => ml.VksSelectionLawunit.LawunitId > 0)
                                                            .Where(ml => ml.VksSelectionLawunit.LawunitId != judgeReporterId)
                                                            .Select(ml => ml.VksSelectionLawunit.LawunitId).ToList()
                                       })
                                       .FirstOrDefault();

            if (selection == null)
            {
                return null;
            }

            var result = new VksSessionDateJudgesVM()
            {
                CourtDepartmentId = selection.CourtDepartmentId,
                IsPredsedatel = selection.IsPredsedatel,
                Judges = new List<VksSessionJudgeVM>()
            };


            foreach (var item in selection.OtherJudges)
            {
                result.Judges.Add(
                    new VksSessionJudgeVM()
                    {
                        LawunitId = item.Value,
                        LawunitUserId = commonService.Users_GetUserIdByLawunit(item.Value)
                    });
            }

            return result;
        }

        public bool IsExistSessionWithoutAct(int CaseId, int SessionId, int SessionTypeId)
        {
            return repo.AllReadonly<CaseSession>()
                       .Any(x => x.CaseId == CaseId &&
                                 x.DateExpired == null &&
                                 x.SessionTypeId == SessionTypeId &&
                                 ((SessionId > 0) ? x.Id != SessionId : true) &&
                                 !x.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                             a.ActTypeId != NomenclatureConstants.ActType.Protokol &&
                                                             a.ActDeclaredDate != null));
        }

        public async Task<bool> IsExistLastSessionWithoutAct(int CaseId, int SessionId)
        {
            var dateTo = DateTime.Now;
            if (SessionId > 0)
            {
                dateTo = await repo.GetPropByIdAsync<CaseSession, DateTime>(x => x.Id == SessionId, x => x.DateFrom);
            }

            var lastSession = await repo.AllReadonly<CaseSession>()
                                        .Include(x => x.CaseSessionActs)
                                        .Where(x => x.CaseId == CaseId &&
                                                    x.DateExpired == null &&
                                                    ((SessionId > 0) ? x.Id != SessionId && x.DateFrom < dateTo : true))
                                        .OrderByDescending(x => x.DateFrom)
                                        .FirstOrDefaultAsync();

            if (lastSession == null)
                return false;

            return !lastSession.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                         a.ActTypeId != NomenclatureConstants.ActType.Protokol &&
                                                         a.ActDeclaredDate != null);
        }
    }
}
