// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using iText.Forms.Xfdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private readonly IUrlHelper urlHelper;

        public CaseSessionService(ILogger<CaseSessionService> _logger,
                                  IRepository _repo,
                                  AutoMapper.IMapper _mapper,
                                  IUserContext _userContext,
                                  ICasePersonService _casePersonService,
                                  IMoneyService _moneyService,
                                  ICaseLifecycleService _caseLifecycleService,
                                  ICaseLawUnitService _caseLawUnitService,
                                  IMQEpepService _mqEpepService,
                                  ICaseDeadlineService _caseDeadlineService,
                                  ICaseLoadIndexService _caseLoadIndexService,
                                  IUrlHelper _url)
        {
            logger = _logger;
            repo = _repo;
            mapper = _mapper;
            userContext = _userContext;
            casePersonService = _casePersonService;
            moneyService = _moneyService;
            caseLifecycleService = _caseLifecycleService;
            caseLawUnitService = _caseLawUnitService;
            caseDeadlineService = _caseDeadlineService;
            mqEpepService = _mqEpepService;
            caseLoadIndexService = _caseLoadIndexService;
            urlHelper = _url;
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
            return repo.AllReadonly<CaseSession>()
                       .Include(x => x.CaseSessionResults)
                       .ThenInclude(x => x.SessionResult)
                       .Include(x => x.Case)
                       .Include(x => x.SessionType)
                       .Include(x => x.CourtHall)
                       .Include(x => x.SessionState)
                       .Include(x => x.CaseSessionActs)
                       .ThenInclude(x => x.ActComplainResult)
                       .Where(x => x.CaseId == CaseId &&
                                   ((DateFrom != null) ? ((x.DateFrom.Date >= (DateFrom ?? DateTime.Now).Date) && (x.DateFrom.Date <= (DateTo ?? DateTime.Now).Date)) : true))
                       .Where(this.FilterExpireInfo<CaseSession>(IsVisibleExpired))
                       .Select(x => new CaseSessionListVM()
                       {
                           Id = x.Id,
                           SessionTypeLabel = (x.SessionType != null) ? x.SessionType.Label : string.Empty,
                           CourtHallName = (x.CourtHall != null) ? x.CourtHall.Name : string.Empty,
                           SessionStateLabel = x.SessionState.Label,
                           DateFrom = x.DateFrom,
                           SessionResultLabel = x.CaseSessionResults.Any(r => r.IsMain && r.DateExpired == null) ? (x.CaseSessionResults.Where(r => r.IsMain && r.DateExpired == null).FirstOrDefault()).SessionResult.Label : string.Empty,
                           ActComplainResultLabel = x.CaseSessionActs.Any(a => a.IsFinalDoc && a.DateExpired == null) ? ((x.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null).FirstOrDefault()).ActComplainResult != null ? (x.CaseSessionActs.Where(a => a.IsFinalDoc && a.DateExpired == null).FirstOrDefault()).ActComplainResult.Label : string.Empty) : string.Empty
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
                .Include(x => x.Case)
                .Include(x => x.SessionType)
                .Include(x => x.CourtHall)
                .Include(x => x.SessionState)
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
                    SessionTypeLabel = (x.SessionType != null) ? x.SessionType.Label : string.Empty,
                    CourtHallName = (x.CourtHall != null) ? x.CourtHall.Name : string.Empty,
                    SessionStateLabel = x.SessionState.Label,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    Description = x.Description,
                    CourtHallId = x.CourtHallId,
                    SessionStateId = x.SessionStateId,
                    SessionTypeId = x.SessionTypeId,
                    DateTo_Minutes = Convert.ToInt32(((TimeSpan)(x.DateTo ?? x.DateFrom).Subtract(x.DateFrom)).TotalMinutes)
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public SaveResultVM CaseSession_SaveData(CaseSessionVM model)
        {
            try
            {
                model.CourtHallId = model.CourtHallId.EmptyToNull();
                model.ActTypeId = model.ActTypeId.NumberEmptyToNull();
                model.ActKindId = model.ActKindId.NumberEmptyToNull();

                var saved = (model.Id > 0) ? repo.GetById<CaseSession>(model.Id) : new CaseSession();
                saved.CaseId = model.CaseId;
                saved.CourtId = model.CourtId;
                saved.SessionTypeId = model.SessionTypeId;
                saved.CourtHallId = model.CourtHallId;

                if (model.ActTypeId != null)
                {
                    if (model.DateFrom <= DateTime.Now)
                        saved.SessionStateId = NomenclatureConstants.SessionState.Provedeno;
                    else
                        saved.SessionStateId = NomenclatureConstants.SessionState.Nasrocheno;
                }
                else
                {
                    saved.SessionStateId = model.SessionStateId;
                }
                
                
                saved.Description = model.Description;
                saved.DateFrom = model.DateFrom;
                saved.DateTo = model.DateFrom.AddMinutes(model.DateTo_Minutes);
                saved.DateWrt = DateTime.Now;
                saved.UserId = userContext.UserId;

                if (model.Id > 0)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        CreateHistory<CaseSession, CaseSessionH>(saved);
                        caseDeadlineService.DeadLineOpenSessionResult(saved);
                        repo.Update(saved);

                        if ((model.SessionStateId != NomenclatureConstants.SessionState.Provedeno) && (model.DateFrom.Date > DateTime.Now.Date))
                        {
                            var caseSessionMeetings = repo.AllReadonly<CaseSessionMeeting>()
                                                          .Where(x => (x.CaseSessionId == saved.Id) &&
                                                                      (x.IsAutoCreate == true))
                                                          .FirstOrDefault();
                            if (caseSessionMeetings != null)
                            {
                                if ((caseSessionMeetings.DateFrom != model.DateFrom) ||
                                    (caseSessionMeetings.DateTo != model.DateTo) ||
                                    (caseSessionMeetings.CourtHallId != model.CourtHallId))
                                {
                                    var dateTimeToMeeteng = (saved.DateFrom.Date > (saved.DateTo ?? saved.DateFrom).Date) ? (new DateTime(saved.DateFrom.Year, saved.DateFrom.Month, saved.DateFrom.Day, 23, 59, 59)) : (saved.DateTo ?? saved.DateFrom);
                                    caseSessionMeetings.DateFrom = saved.DateFrom;
                                    caseSessionMeetings.DateTo = dateTimeToMeeteng;
                                    caseSessionMeetings.CourtHallId = saved.CourtHallId;
                                    caseSessionMeetings.DateWrt = DateTime.Now;
                                    caseSessionMeetings.UserId = userContext.UserId;
                                    repo.Update(caseSessionMeetings);
                                }
                            }
                        }

                        repo.SaveChanges();

                        //Запис на пари за заседатели
                        (bool result, string errorMessage) = moneyService.CalcEarningsJury(saved, userContext.CourtId);
                        if (result == false)
                        {
                            return new SaveResultVM(false, errorMessage);
                        }

                        mqEpepService.AppendCaseSession(saved, EpepConstants.ServiceMethod.Update);

                        ts.Complete();
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope())
                    {

                        CreateHistory<CaseSession, CaseSessionH>(saved);
                        repo.Add<CaseSession>(saved);
                        repo.SaveChanges();

                        DateTime dateEnd = DateTime.Now.AddYears(100);
                        var lawUnits = repo.AllReadonly<CaseLawUnit>()
                                           .Where(x => (x.CaseId == model.CaseId) &&
                                                       (x.CaseSessionId == null) &&
                                                       ((NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) || NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                                       (((x.DateTo ?? dateEnd) >= saved.DateFrom))).ToList();

                        var casePersons = new List<CasePerson>();
                        if (model.CaseSessionOldId != null)
                        {
                            casePersons = repo.AllReadonly<CasePerson>()
                                              .Include(x => x.Addresses)
                                              .ThenInclude(x => x.Address)
                                              .Where(x => (x.CaseId == model.CaseId) &&
                                                          (x.CaseSessionId == model.CaseSessionOldId) &&
                                                          (x.DateExpired == null) &&
                                                          (((x.DateTo ?? dateEnd) >= saved.DateFrom))).ToList();
                        }
                        else
                        {
                            casePersons = repo.AllReadonly<CasePerson>()
                                              .Include(x => x.Addresses)
                                              .ThenInclude(x => x.Address)
                                              .Where(x => (x.CaseId == model.CaseId) &&
                                                          (x.CaseSessionId == null) &&
                                                          (x.DateExpired == null) &&
                                                          (((x.DateTo ?? dateEnd) >= saved.DateFrom))).ToList();
                        }

                        foreach (var lawUnit in lawUnits)
                        {
                            lawUnit.Id = 0;
                            lawUnit.CaseSessionId = saved.Id;
                            //lawUnit.DateFrom = saved.DateFrom;
                            lawUnit.DateWrt = DateTime.Now;
                            lawUnit.UserId = userContext.UserId;
                            repo.Add<CaseLawUnit>(lawUnit);
                        }

                        casePersonService.SetCasePersonDataForCopySession(model.CaseId, null, saved.Id, casePersons, saved.DateFrom);

                        foreach (var item in casePersons)
                            item.PersonRole = repo.GetById<PersonRole>(item.PersonRoleId);

                        var sessionNotificationList = new List<CaseSessionNotificationList>();
                        var maxNumber = 0;

                        foreach (var casePerson in casePersons.OrderBy(x => x.PersonRole.RoleKindId))
                        {
                            maxNumber++;
                            var notificationList = new CaseSessionNotificationList()
                            {
                                CourtId = saved.CourtId,
                                CaseId = saved.CaseId,
                                CaseSessionId = saved.Id,
                                NotificationPersonType = NomenclatureConstants.NotificationPersonType.CasePerson,
                                CasePersonId = casePerson.Id,
                                NotificationAddressId = ((casePerson.Addresses.Count > 0) ? ((casePerson.Addresses.Any(x => (x.ForNotification ?? false) == true)) ? (casePerson.Addresses.Where(x => x.ForNotification == true).FirstOrDefault().AddressId) : (casePerson.Addresses.FirstOrDefault().AddressId)) : (long?)null),
                                RowNumber = maxNumber,
                                DateWrt = DateTime.Now,
                                UserId = userContext.UserId
                            };

                            sessionNotificationList.Add(notificationList);
                        }

                        foreach (var caseLawUnit in lawUnits.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.Jury ||
                                                                        x.JudgeRoleId == NomenclatureConstants.JudgeRole.ReserveJury ||
                                                                        x.JudgeRoleId == NomenclatureConstants.JudgeRole.ExtJury)
                                                            .OrderBy(x => x.LawUnit.FullName))
                        {
                            maxNumber++;

                            var unitAddresses = repo.AllReadonly<LawUnitAddress>()
                                                    .Include(x => x.Address)
                                                    .Where(x => x.LawUnitId == caseLawUnit.LawUnitId)
                                                    .ToList();

                            var notificationList = new CaseSessionNotificationList()
                            {
                                CourtId = saved.CourtId,
                                CaseId = saved.CaseId,
                                CaseSessionId = saved.Id,
                                NotificationPersonType = NomenclatureConstants.NotificationPersonType.CaseLawUnit,
                                CaseLawUnitId = caseLawUnit.Id,
                                NotificationAddressId = (unitAddresses.Count > 0) ? unitAddresses.FirstOrDefault().AddressId : (long?)null,
                                RowNumber = maxNumber,
                                DateWrt = DateTime.Now,
                                UserId = userContext.UserId
                            };

                            sessionNotificationList.Add(notificationList);
                        }

                        foreach (var item in sessionNotificationList)
                        {
                            repo.Add<CaseSessionNotificationList>(item);
                        }

                        var dateTimeToMeeteng = (saved.DateFrom.Date > (saved.DateTo ?? saved.DateFrom).Date) ? (new DateTime(saved.DateFrom.Year, saved.DateFrom.Month, saved.DateFrom.Day, 23, 59, 59)) : (saved.DateTo ?? saved.DateFrom);

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
                            DateWrt = DateTime.Now,
                            UserId = userContext.UserId
                        };
                        repo.Add<CaseSessionMeeting>(caseSessionMeeting);

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
                                        DateWrt = DateTime.Now,
                                        UserId = userContext.UserId
                                    };

                                    if (caseSessionMeetingUser.SecretaryUserId != null)
                                        repo.Add<CaseSessionMeetingUser>(caseSessionMeetingUser);
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
                                ActStateId = NomenclatureConstants.SessionActState.Project,
                                IsFinalDoc = false,
                                IsReadyForPublish = false,
                                DateWrt = DateTime.Now,
                                UserId = userContext.UserId
                            };

                            repo.Add<CaseSessionAct>(caseSessionAct);
                            repo.SaveChanges();
                            model.ActSaveId = caseSessionAct.Id;
                        }

                        repo.SaveChanges();

                        //Запис на пари за заседатели
                        (bool result, string errorMessage) = moneyService.CalcEarningsJury(saved, userContext.CourtId);
                        if (result == false)
                        {
                            return new SaveResultVM(false, errorMessage);
                        }

                        caseDeadlineService.DeadLineOpenSessionResult(saved);
                        repo.SaveChanges();
                        mqEpepService.AppendCaseSession(saved, EpepConstants.ServiceMethod.Add);

                        ts.Complete();
                    }
                }

                model.Id = saved.Id;

                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_CC_SaveData(model.CaseId);

                return new SaveResultVM(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на Заседание Id={ model.Id }");
                return new SaveResultVM(false, MessageConstant.Values.SaveFailed);
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
                    SessionResultLabel = (x.SessionResult != null) ? x.SessionResult.Label : string.Empty,
                    SessionResultId = x.SessionResultId,
                    SessionResultBaseLabel = (x.SessionResultBase != null) ? x.SessionResultBase.Label : string.Empty,
                    IsActiveText = x.IsActive ? MessageConstant.Yes : MessageConstant.No,
                    IsMainText = x.IsMain ? MessageConstant.Yes : MessageConstant.No,
                })
                .AsQueryable();
        }

        /// <summary>
        /// Запис на резултат по заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionResult_SaveData(CaseSessionResult model)
        {
            try
            {
                model.SessionResultBaseId = model.SessionResultBaseId.EmptyToNull();

                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionResult>(model.Id);
                    saved.SessionResultId = model.SessionResultId;
                    saved.SessionResultBaseId = model.SessionResultBaseId;
                    saved.Description = model.Description;
                    saved.IsActive = model.IsActive;
                    saved.IsMain = model.IsMain;
                    repo.Update(saved);
                    caseDeadlineService.DeadLineDeclaredForResolve(saved);
                    caseDeadlineService.DeadLineeOpenSessionResultComplete(saved);
                    caseDeadlineService.DeadLineCompanyCaseCompleteOnResult(saved);
                    repo.SaveChanges();
                    if (mqEpepService.ISPN_IsISPN(model.Case, model.CaseId ?? 0))
                    {
                        mqEpepService.ISPN_CaseSessionResult(saved.Id, ServiceMethod.Add, saved.CaseId);
                    }
                }
                else
                {
                    // Това да се провери после
                    repo.Add<CaseSessionResult>(model);
                    repo.SaveChanges();
                    caseDeadlineService.DeadLineDeclaredForResolve(model);
                    caseDeadlineService.DeadLineeOpenSessionResultComplete(model);
                    caseDeadlineService.DeadLineCompanyCaseCompleteOnResult(model);
                    repo.SaveChanges();
                    if (mqEpepService.ISPN_IsISPN(model.Case, model.CaseId ?? 0))
                    {
                        mqEpepService.ISPN_CaseSessionResult(model.Id, ServiceMethod.Add, model.CaseId);
                    }
                }

                if (model.SessionResultId == NomenclatureConstants.CaseSessionResult.ScheduledFirstSession)
                    caseLifecycleService.CaseLifecycle_SaveFirst_ForCaseType(model.CaseSessionId);

                caseLoadIndexService.CaseLoadIndexAutomationElementGroupe_SRA_SaveData(model.CaseSessionId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на резултат за заседание Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Изчитане на данни по заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public CaseSessionVM CaseSessionVMById(int caseSessionId)
        {
            return repo.AllReadonly<CaseSession>()
                .Include(x => x.Case)
                .Include(x => x.SessionType)
                .Include(x => x.CourtHall)
                .Include(x => x.SessionState)
                .Where(x => x.Id == caseSessionId)
                .Select(x => new CaseSessionVM()
                {
                    Id = x.Id,
                    CourtId = x.CourtId,
                    CaseId = x.CaseId,
                    CaseName = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                    SessionTypeLabel = (x.SessionType != null) ? x.SessionType.Label : string.Empty,
                    CourtHallName = (x.CourtHall != null) ? x.CourtHall.Name : string.Empty,
                    SessionStateLabel = x.SessionState.Label,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    Description = x.Description,
                    CourtHallId = x.CourtHallId,
                    SessionStateId = x.SessionStateId,
                    SessionTypeId = x.SessionTypeId,
                    IsExpired = x.DateExpired != null,
                    DateTo_Minutes = Convert.ToInt32(((TimeSpan)(x.DateTo ?? x.DateFrom).Subtract(x.DateFrom)).TotalMinutes),
                    CaseTypeId = x.Case.CaseTypeId
                })
                .FirstOrDefault();
        }

        /// <summary>
        /// Копиране на заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSession_CopyData(CaseSessionVM model)
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

                var result = CaseSession_SaveData(model);

                return result.Result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при копиране на Заседание Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Справка за заетост на зали
        /// </summary>
        /// <param name="CourtId"></param>
        /// <param name="CourtHallId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateТо"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionHallUseVM> CaseSessionHallUse_Select(int CourtId, int? CourtHallId, DateTime? DateFrom, DateTime? DateТо)
        {
            CourtHallId = CourtHallId.EmptyToNull();
            var caseSessionHallUseVMs = repo.AllReadonly<CaseSessionMeeting>()
                                            .Include(x => x.CourtHall)
                                            .Include(x => x.SessionMeetingType)
                                            .Include(x => x.Case)
                                            .Include(x => x.CaseSession)
                                            .ThenInclude(x => x.SessionType)
                                            .Include(x => x.CaseSession)
                                            .ThenInclude(x => x.SessionState)
                                            .Where(x => (x.Case.CourtId == CourtId) &&
                                                        (x.DateExpired == null) &&
                                                        (x.DateFrom >= (DateFrom ?? DateTime.Now)) &&
                                                        (x.DateFrom <= (DateТо ?? DateTime.Now.AddDays(30))) &&
                                                        ((CourtHallId != null) ? (x.CourtHallId == CourtHallId) : (x.CourtHallId != null)))
                                            .OrderBy(x => x.CourtHall)
                                            .ThenBy(x => x.Id)
                                            .ThenBy(x => x.DateFrom)
                                            .Select(x => new CaseSessionHallUseVM()
                                            {
                                                Id = x.Id,
                                                CaseId = x.CaseId ?? 0,
                                                CaseSessionId = x.CaseSessionId,
                                                SessionLabel = x.SessionMeetingType.Label +
                                                               " " + x.CaseSession.SessionType.Label + " " + x.DateFrom.ToString("dd.MM.yyyy"),
                                                CaseName = x.Case.RegNumber + "/" + x.Case.RegDate.ToString("dd.MM.yyyy"),
                                                CourtHallName = (x.CourtHall != null) ? x.CourtHall.Name : string.Empty,
                                                DateFrom = x.DateFrom,
                                                DateTo = x.DateTo,
                                                CourtHallId = x.CourtHallId,
                                                SessionTypeLabel = x.CaseSession.SessionType.Label,
                                                CourtHallLocation = (x.CourtHall != null) ? x.CourtHall.Location : string.Empty
                                            })
                                            .ToList();

            return caseSessionHallUseVMs.AsQueryable();
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
                       .Where(x => x.CaseSession.CaseId == CaseId)
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

        /// <summary>
        /// Извличане на данни за резултати от заседание
        /// </summary>
        /// <param name="CaseId"></param>
        /// <returns></returns>
        public List<SelectListItem> CaseSessionResultStringList_SelectByCaseId(int CaseId)
        {
            var caseSessionResults = CaseSessionResult_SelectByCaseId(CaseId);
            List<SelectListItem> selectListItems = new List<SelectListItem>();

            foreach (var caseSessionResult in caseSessionResults)
            {
                var selectListItem = selectListItems.Where(x => x.Value == caseSessionResult.Id.ToString()).FirstOrDefault();

                if (selectListItem == null)
                {
                    SelectListItem item = new SelectListItem()
                    {
                        Text = caseSessionResult.SessionResultLabel + ((caseSessionResult.SessionResultBaseLabel != string.Empty) ? " - " + caseSessionResult.SessionResultBaseLabel : string.Empty) + ";",
                        Value = caseSessionResult.CaseSessionId.ToString()
                    };
                    selectListItems.Add(item);
                }
                else
                    selectListItem.Text += caseSessionResult.SessionResultLabel + ((caseSessionResult.SessionResultBaseLabel != string.Empty) ? " - " + caseSessionResult.SessionResultBaseLabel : string.Empty) + ";";
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
        public bool CourtHallBusy(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int ModelId)
        {
            DateTime DateTo = DateFrom.AddMinutes(DateTo_Minutes);
            return repo.AllReadonly<CaseSession>()
                       .Any(x => (x.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno) &&
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
            DateTime dateFromSearch = model.DateFrom == null ? DateTime.Now.AddYears(-100) : (DateTime)model.DateFrom;
            DateTime dateToSearch = model.DateTo == null ? DateTime.Now.AddYears(100) : (DateTime)model.DateTo;
            DateTime dateEnd = DateTime.Now.AddYears(100);

            Expression<Func<CaseSession, bool>> dateSearch = x => true;
            if (model.DateFrom != null || model.DateTo != null)
                dateSearch = x => x.DateFrom.Date >= dateFromSearch.Date && x.DateFrom.Date <= dateToSearch.Date;

            Expression<Func<CaseSession, bool>> yearWhere = x => true;
            if ((model.Year ?? 0) > 0)
                yearWhere = x => x.DateFrom.Year == model.Year;

            Expression<Func<CaseSession, bool>> sessionTypeWhere = x => true;
            if (model.CaseSessionTypeId > 0)
                sessionTypeWhere = x => x.SessionTypeId == model.CaseSessionTypeId;

            Expression<Func<CaseSession, bool>> hallWhere = x => true;
            if (model.HallId > 0)
                hallWhere = x => x.CourtHallId == model.HallId;

            Expression<Func<CaseSession, bool>> secretaryWhere = x => true;
            if (string.IsNullOrEmpty(model.SecretaryUserId) == false && model.SecretaryUserId != "0")
                secretaryWhere = x => x.CaseSessionMeetings.Where(m => m.CaseSessionMeetingUsers.Where(u => u.SecretaryUserId == model.SecretaryUserId).Any()).Any();

            Expression<Func<CaseSession, bool>> caseGroupWhere = x => true;
            if (model.CaseGroupId > 0)
                caseGroupWhere = x => x.Case.CaseGroupId == model.CaseGroupId;

            Expression<Func<CaseSession, bool>> caseTypeWhere = x => true;
            if (model.CaseTypeId > 0)
                caseTypeWhere = x => x.Case.CaseTypeId == model.CaseTypeId;

            Expression<Func<CaseSession, bool>> caseSessionResultWhere = x => true;
            if (model.SessionResultId > 0)
                caseSessionResultWhere = x => x.CaseSessionResults.Any(res => res.SessionResultId == model.SessionResultId);

            Expression<Func<CaseSession, bool>> sessionStateWhere = x => true;
            if (model.SessionStateId > 0)
                sessionTypeWhere = x => x.SessionStateId == model.SessionStateId;

            Expression<Func<CaseSession, bool>> caseRegnumberSearch = x => true;
            if (!string.IsNullOrEmpty(model.RegNumber))
                caseRegnumberSearch = x => x.Case.RegNumber.ToLower().EndsWith(model.RegNumber.ToShortCaseNumber().ToLower());

            Expression<Func<CaseSession, bool>> courtDepartment = x => true;
            if (model.CourtDepartmentId > 0)
                courtDepartment = x => x.CaseLawUnits.Any(a => a.CourtDepartmentId == model.CourtDepartmentId);

            Expression<Func<CaseSession, bool>> judgeReporterSearch = x => true;
            if (model.JudgeReporterId > 0)
                judgeReporterSearch = x => x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd).Date >= x.DateFrom.Date && a.LawUnitId == model.JudgeReporterId &&
                                                                     a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any();

            return repo.AllReadonly<CaseSession>()
                       .Include(x => x.CaseLawUnits)
                       .ThenInclude(x => x.LawUnit)
                       .Include(x => x.CaseLawUnits)
                       .ThenInclude(x => x.CourtDepartment)
                       .Include(x => x.Case)
                       .Include(x => x.SessionType)
                       .Include(x => x.CourtHall)
                       .Include(x => x.SessionState)
                       .Include(x => x.CaseSessionResults)
                       .Where(x => x.Case.CourtId == userContext.CourtId)
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
                       .Select(x => new CaseSessionVM()
                       {
                           Id = x.Id,
                           CourtId = x.CourtId,
                           CaseId = x.CaseId,
                           CaseName = x.Case.RegNumber,
                           CaseTypeLabel = x.Case.CaseType.Code,
                           SessionTypeLabel = (x.SessionType != null) ? x.SessionType.Label : string.Empty,
                           CourtHallName = (x.CourtHall != null) ? x.CourtHall.Name : string.Empty,
                           SessionStateLabel = x.SessionState.Label,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           Description = x.Description,
                           CourtHallId = x.CourtHallId,
                           SessionStateId = x.SessionStateId,
                           SessionTypeId = x.SessionTypeId,
                           DateTo_Minutes = Convert.ToInt32(((TimeSpan)(x.DateTo ?? x.DateFrom).Subtract(x.DateFrom)).TotalMinutes),
                           SessionResultLabel = string.Join(",", x.CaseSessionResults.Where(r => r.DateExpired == null).Select(r => r.SessionResult.Label + (r.SessionResultBaseId != null ? " - " + r.SessionResultBase.Label : string.Empty))),
                           JudgeReporterLabel = x.CaseLawUnits.Where(a => (a.DateTo ?? dateEnd).Date >= x.DateFrom.Date && a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Select(a => a.LawUnit.FullName + (a.CourtDepartment != null ? " (" + a.CourtDepartment.Label + ")" : string.Empty)).FirstOrDefault()
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извлича обвиняемите по заседание
        /// </summary>
        /// <param name="caseSessionTimeBook"></param>
        private void FillLeftRightSide(CaseSessionTimeBookVM caseSessionTimeBook)
        {
            var casePersonLists = casePersonService.CasePerson_Select(caseSessionTimeBook.CaseId, caseSessionTimeBook.Id).ToList();
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
            var caseSessionResults = CaseSessionResult_Select(caseSessionTimeBook.Id);
            var caseSessionActs = repo.AllReadonly<CaseSessionAct>()
                                      .Include(x => x.ActType)
                                      .Include(x => x.ActState)
                                      .Where(x => x.CaseSessionId == caseSessionTimeBook.Id)
                                      .ToList();

            foreach (var caseSessionResult in caseSessionResults)
            {
                caseSessionTimeBook.ResultAndAct += caseSessionResult.SessionResultLabel + ((caseSessionResult.SessionResultBaseLabel != string.Empty) ? " - " + caseSessionResult.SessionResultBaseLabel + "; " : "; ");
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
                                               ((DateTo >= x.DateFrom) && (DateFrom <= x.DateTo)));

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
            List<CaseSessionSprVM> result = new List<CaseSessionSprVM>();

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);

            var casesSessions = repo.AllReadonly<CaseSession>()
                                    .Include(x => x.SessionType)
                                    .Include(x => x.CasePersons)
                                    .ThenInclude(x => x.PersonMaturity)
                                    .Include(x => x.CasePersons)
                                    .ThenInclude(x => x.PersonRole)
                                    .Include(x => x.CaseLawUnits)
                                    .ThenInclude(x => x.LawUnit)
                                    .Include(x => x.CaseLawUnits)
                                    .ThenInclude(x => x.JudgeRole)
                                    .Include(x => x.CaseLawUnits)
                                    .ThenInclude(x => x.CourtDepartment)
                                    .Include(x => x.Case)
                                    .ThenInclude(x => x.CaseGroup)
                                    .Include(x => x.Case)
                                    .ThenInclude(x => x.CaseType)
                                    .Include(x => x.Case)
                                    .ThenInclude(x => x.CaseCode)
                                    .Include(x => x.CaseSessionResults)
                                    .ThenInclude(x => x.SessionResult)
                                    .Where(x => (x.CourtId == courtId) &&
                                                ((x.DateFrom >= model.DateFrom) && (x.DateFrom <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                                (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                                (x.CasePersons.Any(p => p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge)) &&
                                                (model.SessionTypeId > 0 ? x.SessionTypeId == model.SessionTypeId : true) &&
                                                ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                                ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                                ((model.JudgeReporterId > 0) ? (x.CaseLawUnits.Where(a => (a.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.Case.RegDate.Date && a.LawUnitId == model.JudgeReporterId &&
                                                                                                          a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any()) : true))
                                    .ToList();

            foreach (var session in casesSessions)
            {
                var judgeRep = session.CaseLawUnits.Where(x => x.CaseSessionId == session.Id && (x.DateTo ?? DateTime.Now.AddYears(100)).Date >= session.Case.RegDate.Date && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
                var act = repo.AllReadonly<CaseSessionAct>()
                              .Where(x => x.CaseSessionId == session.Id &&
                                          x.IsFinalDoc)
                              .FirstOrDefault();
                var res = session.CaseSessionResults.Where(x => x.IsMain && x.DateExpired == null).FirstOrDefault();

                var item = new CaseSessionSprVM();
                item.CaseGroupLabel = session.Case.CaseGroup.Label + " - " + session.Case.CaseType.Label;
                item.CaseRegNum = session.Case.RegNumber + "/" + session.Case.RegDate.Year.ToString() + "г.";
                item.CaseRegDate = session.Case.RegDate;
                item.JudgeReport = (judgeRep != null) ? judgeRep.LawUnit.FullName + ((judgeRep.CourtDepartment != null) ? " състав: " + judgeRep.CourtDepartment.Label : string.Empty) : string.Empty;
                item.SessionTypeLabel = session.SessionType.Label;
                item.SessionDateFrom = session.DateFrom;
                item.CasePersons = GetStringPerson(session.CasePersons.Where(p => p.CaseSessionId == session.Id && (p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderAged || p.PersonMaturityId == NomenclatureConstants.PersonMaturity.UnderLegalAge)).ToList());
                item.ActDeclaredDate = (act != null) ? act.ActDeclaredDate : null;
                item.SessionResultLabel = (res != null) ? res.SessionResult.Label : string.Empty;
                item.Id = session.Id;
                item.CaseId = session.CaseId;
                item.SessionDateReturn = (act != null) ? act.RegDate : null;
                item.SessionDateEntryIntoForce = (act != null) ? act.ActInforcedDate : null;

                result.Add(item);
            }

            return result.AsQueryable();
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
                             .Where(x => x.CaseId == caseSession.CaseId && x.DateFrom < caseSession.DateFrom)
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
        /// <param name="courtId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public IQueryable<CaseSprVM> CaseSessionWithActProject_Select(int courtId, CaseFilterReport model)
        {
            List<CaseSprVM> result = new List<CaseSprVM>();

            model.DateFrom = NomenclatureExtensions.ForceStartDate(model.DateFrom);
            model.DateTo = NomenclatureExtensions.ForceEndDate(model.DateTo);

            var caseSessions = repo.AllReadonly<CaseSession>()
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseGroup)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseType)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseCode)
                                   .Include(x => x.Case)
                                   .ThenInclude(x => x.CaseLawUnits)
                                   .ThenInclude(x => x.LawUnit)
                                   .Include(x => x.CaseLawUnits)
                                   .ThenInclude(x => x.LawUnit)
                                   .Include(x => x.CaseLawUnits)
                                   .ThenInclude(x => x.JudgeRole)
                                   .Include(x => x.CaseLawUnits)
                                   .ThenInclude(x => x.CourtDepartment)
                                   .Include(x => x.SessionType)
                                   .Include(x => x.CaseSessionActs)
                                   .Where(x => (x.CourtId == courtId) &&
                                            ((x.DateFrom >= model.DateFrom) && (x.DateTo <= (model.DateTo ?? DateTime.Now.AddYears(100)))) &&
                                            (x.Case.CaseStateId != NomenclatureConstants.CaseState.Draft) &&
                                            ((model.CaseGroupId > 0) ? x.Case.CaseGroupId == model.CaseGroupId : true) &&
                                            ((model.CaseTypeId > 0) ? x.Case.CaseTypeId == model.CaseTypeId : true) &&
                                            ((model.CaseCodeId > 0) ? x.Case.CaseCodeId == model.CaseCodeId : true) &&
                                            (x.SessionStateId == NomenclatureConstants.SessionState.Provedeno) &&
                                            (x.CaseSessionActs.Any(b => (b.ActStateId == NomenclatureConstants.SessionActState.Project) && (b.DateExpired == null)) ||
                                             !x.CaseSessionActs.Any(b => (b.DateExpired == null)) ||
                                             x.CaseSessionActs.Any(b => b.DateExpired == null && b.ActDeclaredDate > model.ActDateToSpr)) &&
                                            ((model.JudgeReporterId > 0) ? (x.CaseLawUnits.Where(a => (a.DateTo ?? DateTime.Now.AddYears(100)).Date >= x.DateFrom && a.LawUnitId == model.JudgeReporterId &&
                                                                                                      a.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Any()) : true))
                                   .ToList();

            foreach (var caseSession in caseSessions)
            {
                var judgeRep = caseSession.CaseLawUnits.Where(x => (x.DateTo ?? DateTime.Now.AddYears(100)).Date >= caseSession.DateFrom && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();

                DateTime? sessionActDate = null;
                if (caseSession.CaseSessionActs.Count > 0)
                {
                    var caseSessionAct = caseSession.CaseSessionActs.Where(x => x.ActStateId == NomenclatureConstants.SessionActState.Project && x.DateExpired == null).FirstOrDefault();
                    if (caseSessionAct != null)
                        sessionActDate = caseSessionAct.RegDate;
                    else
                    {
                        caseSessionAct = caseSession.CaseSessionActs.Where(x => x.ActDeclaredDate > model.ActDateToSpr && x.DateExpired == null).FirstOrDefault();
                        if (caseSessionAct != null)
                            sessionActDate = caseSessionAct.RegDate;
                    }
                }

                var item = new CaseSprVM()
                {
                    Id = caseSession.CaseId,
                    JudgeReport = (judgeRep != null) ? judgeRep.LawUnit.FullName + ((judgeRep.CourtDepartment != null) ? " състав: " + judgeRep.CourtDepartment.Label : string.Empty) : string.Empty,
                    CaseTypeLabel = caseSession.Case.CaseType.Label,
                    CaseRegNum = caseSession.Case.RegNumber + "/" + caseSession.Case.RegDate.Year.ToString() + "г.",
                    CaseCodeLabel = caseSession.Case.CaseCode.Code,
                    SessionTypeLabel = caseSession.SessionType.Label,
                    SessionDateFrom = caseSession.DateFrom,
                    SessionActDate = sessionActDate
                };

                result.Add(item);
            }

            return result.AsQueryable();
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

            var sessions = CaseSessionHallUse_Select(CourtId, CourtHallId, DateFrom, DateТо).Select(x => new CalendarVM()
            {
                id = x.Id,
                title = x.SessionTypeLabel,
                start = x.DateFrom,
                end = x.DateTo,
                color = "#00c0ef",
                pop_content = x.CaseName + (string.IsNullOrEmpty(x.CourtHallName) ? " " + x.CourtHallName + (string.IsNullOrEmpty(x.CourtHallLocation) ? " " + x.CourtHallLocation : string.Empty) : string.Empty),
                pop_title = "Дело",
                SourceId = x.CaseSessionId
            }).ToList();

            foreach (var item in sessions)
            {
                item.url = urlHelper.Action("Preview", "CaseSession", new { id = item.SourceId });
            }
            result.AddRange(sessions);
            return result;
        }
    }
}
