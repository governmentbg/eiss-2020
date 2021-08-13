using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using Microsoft.AspNetCore.Mvc.Rendering;
using IOWebApplication.Infrastructure.Constants;
using NPOI.SS.Formula.Functions;
using IOWebApplication.Infrastructure.Extensions;
using Remotion.Linq.Clauses;
using System.Transactions;
using IOWebApplication.Core.Helper;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionMeetingService : BaseService, ICaseSessionMeetingService
    {
        private readonly ICaseDeadlineService caseDeadlineService;
        private readonly IMoneyService moneyService;
        private readonly IMQEpepService mqService;

        public CaseSessionMeetingService(
            ILogger<CaseSessionMeetingService> _logger,
            IUserContext _userContext,
            IRepository _repo,
            ICaseDeadlineService _caseDeadlineService,
            IMQEpepService _mqService,
            IMoneyService _moneyService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            caseDeadlineService = _caseDeadlineService;
            moneyService = _moneyService;
            mqService = _mqService;
        }

        /// <summary>
        /// Извличане на данни за Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="IsVisibleExpired"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionMeetingVM> CaseSessionMeeting_Select(int caseSessionId, bool IsVisibleExpired = false)
        {
            return repo.AllReadonly<CaseSessionMeeting>()
                       .Include(x => x.SessionMeetingType)
                       .Include(x => x.CourtHall)
                       .Include(x => x.CaseSessionMeetingUsers)
                       .ThenInclude(x => x.SecretaryUser)
                       .ThenInclude(x => x.LawUnit)
                       .Where(x => x.CaseSessionId == caseSessionId)
                       .Where(this.FilterExpireInfo<CaseSessionMeeting>(IsVisibleExpired))
                       .Select(x => new CaseSessionMeetingVM()
                       {
                           Id = x.Id,
                           CaseSessionId = x.CaseSessionId,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           Description = x.Description,
                           SessionMeetingTypeLabel = x.SessionMeetingType.Label,
                           IsAutoCreate = x.IsAutoCreate,
                           CourtHallLabel = (x.CourtHall != null ? x.CourtHall.Name : string.Empty),
                           SecretaryUserNames = string.Join(",", x.CaseSessionMeetingUsers.Select(u => u.SecretaryUser.LawUnit.FullName))
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionMeeting_SaveData(CaseSessionMeetingEditVM model)
        {
            try
            {
                model.CourtHallId = model.CourtHallId.EmptyToNull();
                var modelSave = FillCaseSessionMeeting(model);
                using (TransactionScope ts = TransactionScopeBuilder.CreateReadCommitted())
                {
                    if (model.Id > 0)
                    {
                        //Update
                        var saved = repo.GetById<CaseSessionMeeting>(modelSave.Id);
                        saved.SessionMeetingTypeId = modelSave.SessionMeetingTypeId;
                        saved.CourtHallId = model.CourtHallId;
                        saved.DateFrom = modelSave.DateFrom;
                        saved.DateTo = modelSave.DateTo;
                        saved.Description = modelSave.Description;
                        saved.DateWrt = DateTime.Now;
                        saved.UserId = userContext.UserId;
                        repo.Update(saved);

                        var caseSessionMeetingUsers = repo.AllReadonly<CaseSessionMeetingUser>()
                                                          .Where(x => x.CaseSessionMeetingId == modelSave.Id)
                                                          .ToList();
                        repo.DeleteRange<CaseSessionMeetingUser>(caseSessionMeetingUsers);
                    }
                    else
                    {
                        //Insert
                        modelSave.DateWrt = DateTime.Now;
                        modelSave.UserId = userContext.UserId;
                        repo.Add<CaseSessionMeeting>(modelSave);
                    }

                    if (model.CaseSessionMeetingUser != null)
                    {
                        foreach (var checkedItem in model.CaseSessionMeetingUser.Where(x => x.Checked))
                        {
                            var userSave = new CaseSessionMeetingUser()
                            {
                                CourtId = modelSave.CourtId,
                                CaseId = modelSave.CaseId,
                                CaseSessionMeetingId = modelSave.Id,
                                SecretaryUserId = checkedItem.Value,
                                DateWrt = DateTime.Now,
                                UserId = userContext.UserId
                            };

                            repo.Add(userSave);
                        }
                    }

                    if (model.IsAutoCreate ?? false)
                    {
                        if (model.SessionStateId != null)
                        {
                            var caseSessionUpdate = repo.GetById<CaseSession>(model.CaseSessionId);
                            var savedSessionState = caseSessionUpdate.SessionStateId;
                            caseSessionUpdate.SessionStateId = model.SessionStateId ?? 0;
                            caseSessionUpdate.DateWrt = DateTime.Now;
                            caseSessionUpdate.UserId = userContext.UserId;
                            repo.Update(caseSessionUpdate);
                            mqService.AppendCaseSession(caseSessionUpdate, EpepConstants.ServiceMethod.Update);

                            //CBorisoff,29.07.2021
                            //когато заседанието е насрочено и се промени на проведено 
                            //се изпращат всички постановени актове в него към външните системи
                            if (savedSessionState == NomenclatureConstants.SessionState.Nasrocheno
                                && model.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                            {
                                mqService.AppendActsFromSession(model.CaseSessionId);
                            }
                        }
                    }

                    repo.SaveChanges();

                    //Запис на пари за заседатели
                    var caseSession = repo.AllReadonly<CaseSession>()
                                      .Where(x => x.Id == model.CaseSessionId)
                                      .FirstOrDefault();
                    (bool result, string errorMessage) = moneyService.CalcEarningsJury(caseSession, userContext.CourtId);
                    if (result == false)
                    {
                        return false;
                    }

                    if (model.Id < 1)
                        model.Id = modelSave.Id;

                    ts.Complete();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на тайно съвещание Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за секретари към Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="CaseSessionMeetingId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionMeetingUserVM> CaseSessionMeetingUser_Select(int CaseSessionMeetingId)
        {
            return repo.AllReadonly<CaseSessionMeetingUser>()
                       .Include(x => x.SecretaryUser)
                       .ThenInclude(x => x.LawUnit)
                       .Where(x => x.CaseSessionMeetingId == CaseSessionMeetingId)
                       .Select(x => new CaseSessionMeetingUserVM()
                       {
                           Id = x.Id,
                           SecretaryUserName = x.SecretaryUser.LawUnit.FullName,
                           DateWrt = x.DateWrt
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Запис на секретари към Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionMeetingUser_SaveData(CaseSessionMeetingUser model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionMeetingUser>(model.Id);
                    saved.SecretaryUserId = model.SecretaryUserId;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    caseDeadlineService.DeadLineOpenSessionResult(saved);
                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    caseDeadlineService.DeadLineOpenSessionResult(model);
                    repo.Add<CaseSessionMeetingUser>(model);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на секретари към сесии на заседание Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни по ид на Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CaseSessionMeeting CaseSessionMeeting_ById(int Id)
        {
            return repo.AllReadonly<CaseSessionMeeting>()
                       .Include(x => x.SessionMeetingType)
                       .Where(x => x.Id == Id)
                       .FirstOrDefault();
        }

        /// <summary>
        /// Извличане на данни за секретари от Тайни съвещания/сесии към заседание за комбо
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="addDefaultElement"></param>
        /// <param name="addAllElement"></param>
        /// <returns></returns>
        public List<SelectListItem> GetDDL_MeetingUserBySessionId(int caseSessionId, bool addDefaultElement = true, bool addAllElement = false)
        {
            var selectListItems = repo.AllReadonly<CaseSessionMeeting>()
                                      .Where(x => x.CaseSessionId == caseSessionId)
                                      .SelectMany(x => x.CaseSessionMeetingUsers)
                                      .Where(x => x.SecretaryUserId != null)
                                      .Select(x => new
                                      {
                                          val = x.SecretaryUserId.ToString(),
                                          txt = x.SecretaryUser.LawUnit.FullName
                                      })
                                      .Distinct()
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.txt,
                                          Value = x.val
                                      }).ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Избери", Value = string.Empty })
                    .ToList();
            }

            if (addAllElement)
            {
                selectListItems = selectListItems
                    .Prepend(new SelectListItem() { Text = "Всички", Value = string.Empty })
                    .ToList();
            }

            return selectListItems;
        }

        /// <summary>
        /// Извличане на Тайни съвещания/сесии към заседание по дело
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionMeetingVM> CaseSessionMeeting_SelectByCaseId(int caseId)
        {
            return repo.AllReadonly<CaseSessionMeeting>()
                       .Include(x => x.SessionMeetingType)
                       .Include(x => x.CaseSession)
                       .Where(x => x.CaseSession.CaseId == caseId)
                       .Select(x => new CaseSessionMeetingVM()
                       {
                           Id = x.Id,
                           CaseSessionId = x.CaseSessionId,
                           DateFrom = x.DateFrom,
                           DateTo = x.DateTo,
                           Description = x.Description,
                           SessionMeetingTypeLabel = x.SessionMeetingType.Label
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Попълване обект за редакция на Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private CaseSessionMeetingEditVM FillCaseSessionMeetingEditVM(CaseSessionMeeting model)
        {
            return new CaseSessionMeetingEditVM()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                CaseSessionId = model.CaseSessionId,
                SessionMeetingTypeId = model.SessionMeetingTypeId,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                Description = model.Description,
                IsActive = model.IsActive,
                IsAutoCreate = model.IsAutoCreate,
                CourtHallId = model.CourtHallId,
                IsSessionProvedeno = (model.CaseSession == null) ? false : (model.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno),
                SessionStateId = (model.CaseSession == null) ? (int?)null : model.CaseSession.SessionStateId
            };
        }

        /// <summary>
        /// Попълване на основен обект за запис на Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private CaseSessionMeeting FillCaseSessionMeeting(CaseSessionMeetingEditVM model)
        {
            return new CaseSessionMeeting()
            {
                Id = model.Id,
                CourtId = model.CourtId,
                CaseId = model.CaseId,
                CaseSessionId = model.CaseSessionId,
                SessionMeetingTypeId = model.SessionMeetingTypeId,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                Description = model.Description,
                IsActive = model.IsActive,
                IsAutoCreate = model.IsAutoCreate,
                CourtHallId = model.CourtHallId
            };
        }

        /// <summary>
        /// Извличане по ид на данни за Тайни съвещания/сесии към заседание в обект за редакция
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public CaseSessionMeetingEditVM CaseSessionMeetingEdit_ById(int Id)
        {
            return FillCaseSessionMeetingEditVM(repo.AllReadonly<CaseSessionMeeting>().Include(x => x.CaseSession).Where(x => x.Id == Id).FirstOrDefault());
        }

        /// <summary>
        /// Извличане на данни за секретари за чеклист от Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <param name="CaseSessionMeetingId"></param>
        /// <returns></returns>
        public List<CheckListVM> GetCheckListCaseSessionMeetingUser(int caseSessionId, int CaseSessionMeetingId = 0)
        {
            var caseSession = repo.GetById<CaseSession>(caseSessionId);

            var CaseSessionMeetingUsers = repo.AllReadonly<CaseSessionMeetingUser>()
                                              .Include(x => x.SecretaryUser)
                                              .ThenInclude(x => x.LawUnit)
                                              .Where(x => x.CaseSessionMeetingId == CaseSessionMeetingId)
                                              .ToList();

            var CaselawUnits = repo.AllReadonly<CaseLawUnit>()
                                   .Include(x => x.LawUnit)
                                   .Where(x => x.CaseSessionId == caseSessionId &&
                                               x.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary &&
                                               ((x.DateTo ?? caseSession.DateFrom.AddYears(100)) >= caseSession.DateFrom))
                                   .ToList();

            var result = new List<CheckListVM>();

            foreach (var caseLaw in CaselawUnits)
            {
                var checkElement = new CheckListVM()
                {
                    Checked = CaseSessionMeetingUsers.Any(x => x.SecretaryUserId == caseLaw.LawUnitUserId),
                    Label = caseLaw.LawUnit.FullName,
                    Value = caseLaw.LawUnitUserId
                };
                if (checkElement.Value != null)
                    result.Add(checkElement);
            }

            return result;
        }

        /// <summary>
        /// Проверка за Тайни съвещания/сесии към заседание в зададен период
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="CaseSessionId"></param>
        /// <param name="MeetingId"></param>
        /// <returns></returns>
        public bool IsExistMeetengInSession(DateTime DateFrom, DateTime DateTo, int CaseSessionId, int MeetingId = 0)
        {
            return repo.AllReadonly<CaseSessionMeeting>()
                       .Any(x => (x.CaseSessionId == CaseSessionId) &&
                                 (MeetingId > 0 ? x.Id != MeetingId : true) &&
                                 (x.DateExpired == null) &&
                                 ((DateTo >= x.DateFrom) && (DateFrom <= x.DateTo)));
        }

        /// <summary>
        /// Извличане на данни за автоматична сесия по заседание
        /// </summary>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public CaseSessionMeeting CaseSessionMeetingAutoCreateGetBySessionId(int CaseSessionId)
        {
            return repo.AllReadonly<CaseSessionMeeting>()
                       .Where(x => (x.CaseSessionId == CaseSessionId) &&
                                   ((x.IsAutoCreate ?? false) == true))
                       .FirstOrDefault();
        }

        /// <summary>
        /// Извличане на данни за заетост на зали по Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="CourtHallId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public bool CourtHallBusy(int CourtHallId, DateTime DateFrom, DateTime DateTo, int CaseSessionId)
        {
            return repo.AllReadonly<CaseSessionMeeting>()
                       .Include(x => x.CaseSession)
                       .Any(x => ((CaseSessionId > 0) ? (x.CaseSessionId != CaseSessionId) : true) &&
                                 (x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno) &&
                                 (x.CaseSession.DateExpired == null) &&
                                 (x.CourtHallId == CourtHallId) &&
                                 (x.DateExpired == null) &&
                                 ((DateTo >= x.DateFrom) && (DateFrom <= x.DateTo)));
        }

        /// <summary>
        /// Извличане на данни за заетост на зала за заседание
        /// </summary>
        /// <param name="CourtHallId"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo_Minutes"></param>
        /// <param name="CaseSessionId"></param>
        /// <returns></returns>
        public bool CourtHallBusyFromSession(int CourtHallId, DateTime DateFrom, int DateTo_Minutes, int CaseSessionId)
        {
            DateTime DateTo = DateFrom.AddMinutes(DateTo_Minutes);
            return CourtHallBusy(CourtHallId, DateFrom, DateTo, CaseSessionId);
        }

        /// <summary>
        /// Проверка за заетост на състав от дело по Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="dateTimeFrom"></param>
        /// <param name="dateTimeTo"></param>
        /// <returns></returns>
        public string IsCaseLawUnitFromCaseBusy(int caseId, int caseSessionId, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            var caseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                   .Include(x => x.LawUnit)
                                   .Include(x => x.JudgeRole)
                                   .Where(x => ((caseSessionId > 0) ? (x.CaseSessionId == caseSessionId) : (x.CaseId == caseId && x.CaseSessionId == null)) &&
                                               ((x.DateFrom <= dateTimeFrom) && ((x.DateTo ?? dateTimeFrom.AddYears(1)) >= dateTimeFrom)))
                                      .ToList();

            var caseSessionMeetings = repo.AllReadonly<CaseSessionMeeting>()
                                          .Include(x => x.Case)
                                          .Include(x => x.CaseSession)
                                          .ThenInclude(x => x.CaseLawUnits)
                                          .Where(x => (x.CaseSessionId != caseSessionId) &&
                                                      (x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno) &&
                                                      (x.CaseSession.DateExpired == null) &&
                                                      (x.DateExpired == null) &&
                                                      ((dateTimeTo >= x.DateFrom) && (dateTimeFrom <= x.DateTo)))
                                          .ToList();

            var result = string.Empty;

            foreach (var caseLaw in caseLawUnits)
            {
                foreach (var caseSessionMeeting in caseSessionMeetings.Where(x => x.CaseSession.CaseLawUnits.Any(l => l.LawUnitId == caseLaw.LawUnitId)))
                {
                    result += caseLaw.LawUnit.FullName + " (" + caseLaw.JudgeRole.Label + ") има застъпване със сесия от дело: " + caseSessionMeeting.Case.RegNumber + "/" + caseSessionMeeting.Case.RegDate.ToString("dd.MM.yyyy") + "; ";
                }
            }

            return result;
        }

        /// <summary>
        /// Проверка за Тайни съвещания/сесии към заседание след дата
        /// </summary>
        /// <param name="DateTo"></param>
        /// <param name="CaseSessionId"></param>
        /// <param name="CaseSessionMeetingId"></param>
        /// <returns></returns>
        public bool IsExistMeetengInSessionAfterDate(DateTime DateTo, int CaseSessionId, int? CaseSessionMeetingId)
        {
            return repo.AllReadonly<CaseSessionMeeting>()
                       .Any(x => x.CaseSessionId == CaseSessionId &&
                                 (CaseSessionMeetingId != null ? x.Id != CaseSessionMeetingId : true) &&
                                 x.DateTo >= DateTo &&
                                 x.DateExpired == null);
        }

        /// <summary>
        /// Проверка дали има избрани секретар по Тайни съвещания/сесии към заседание
        /// </summary>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public bool CheckExistSecretaryOfAllMeeting(int caseSessionId)
        {
            var caseSession = repo.GetById<CaseSession>(caseSessionId);

            if (caseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno)
                return true;

            return !repo.AllReadonly<CaseSessionMeeting>()
                        .Include(x => x.CaseSessionMeetingUsers)
                        .Any(x => x.CaseSessionId == caseSessionId &&
                                  x.DateExpired == null &&
                                  x.CaseSessionMeetingUsers.Count < 1);
        }
    }
}
