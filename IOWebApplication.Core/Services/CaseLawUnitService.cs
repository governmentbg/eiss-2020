using AutoMapper.QueryableExtensions;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Extensions;
using IOWebApplication.Core.Helper;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IOWebApplication.Core.Services
{
    public class CaseLawUnitService : BaseService, ICaseLawUnitService
    {
        private readonly ICourtLoadPeriodService courtLoadPeriodService;
        private readonly ICommonService commonService;
        private readonly IMQEpepService mqService;

        public CaseLawUnitService(ILogger<CaseLawUnitService> _logger,
                                  IRepository _repo,
                                  IUserContext _userContext,
                                  ICommonService _commonService,
                                  ICourtLoadPeriodService _courtLoadPeriodService,
                                  IMQEpepService _mqService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            commonService = _commonService;
            courtLoadPeriodService = _courtLoadPeriodService;
            mqService = _mqService;
        }

        /// <summary>
        /// Извличане на данни за съдебен състав по дело/заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <param name="allData"></param>
        /// <param name="isManualRoles"></param>
        /// <returns></returns>
        public IQueryable<CaseLawUnitVM> CaseLawUnit_Select(int caseId, int? caseSessionId, bool allData = false, bool isManualRoles = false)
        {
            var date = ((caseSessionId ?? 0) == 0) ? DateTime.Now : repo.GetById<CaseSession>(caseSessionId).DateFrom;
            var caseLawUnitVMs = repo.AllReadonly<CaseLawUnit>()
                                     .Include(x => x.LawUnit)
                                     .Include(x => x.JudgeRole)
                                     .Include(x => x.CourtDepartment)
                                     .Include(x => x.JudgeDepartmentRole)
                                     .Include(x => x.CaseSession)
                                     .ThenInclude(x => x.SessionType)
                                     .Include(x => x.LawUnitSubstitution)
                                     .Where(x => x.CaseId == caseId &&
                                                 (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) &&
                                                 ((!allData) ? ((x.DateTo ?? date.AddYears(1)) >= date) : true) &&
                                                 (isManualRoles ? (NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) : (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId))))
                                     .Select(x => new CaseLawUnitVM()
                                     {
                                         Id = x.Id,
                                         CaseId = x.CaseId,
                                         CaseSessionId = x.CaseSessionId,
                                         LawUnitId = x.LawUnitId,
                                         JudgeRoleId = x.JudgeRoleId,
                                         JudgeRoleLabel = (x.JudgeRole != null) ? x.JudgeRole.Label : string.Empty,
                                         JudgeDepartmentRoleId = x.JudgeDepartmentRoleId,
                                         JudgeDepartmentRoleLabel = (x.JudgeDepartmentRole != null) ? x.JudgeDepartmentRole.Label : string.Empty,
                                         LawUnitName = (x.LawUnit != null) ? x.LawUnit.FullName : string.Empty,
                                         LawUnitNameShort = (x.LawUnit != null) ? x.LawUnit.FullName_MiddleNameInitials : string.Empty,
                                         DepartmentLabel = (x.CourtDepartment != null) ? " " + x.CourtDepartment.Label : string.Empty,
                                         DateFrom = x.DateFrom,
                                         DateTo = x.DateTo,
                                         CaseSessionLabel = ((x.CaseSessionId != null) ? (x.CaseSession.SessionType.Label + " " + x.CaseSession.DateFrom.ToString("dd.MM.yyyy HH:mm")) : (string.Empty)),
                                         DepartmentId = x.CourtDepartmentId,
                                         SubstitutionId = x.LawUnitSubstitutionId,
                                         //Взема LawUnitId на заместеното лице, ако е по заместване или самия съдия от заседанието
                                         SubstitutedLawUnitId = (x.LawUnitSubstitution != null) ? x.LawUnitSubstitution.LawUnitId : x.LawUnitId,
                                         SubstitutedLawUnitName = (x.LawUnitSubstitution != null) ? x.LawUnitSubstitution.LawUnit.FullName : string.Empty
                                     })
                                     .ToList();


            foreach (var caseLaw in caseLawUnitVMs)
            {
                var dismisal = CaseLawUnitDismisal_GetByCaseLawUnitId(caseLaw.Id);
                caseLaw.IsExistDismisal = (dismisal != null);
                caseLaw.DismisalLabel = (dismisal != null) ? "Има" : "Няма";
                caseLaw.DismisalLabelFull = (dismisal != null) ? dismisal.DismisalType.Label + " " + dismisal.DismisalDate.ToString("dd.MM.yyyy") + " " : string.Empty;
                caseLaw.RowLabelFull = caseLaw.LawUnitName + " (" + caseLaw.JudgeRoleLabel + ") от " + caseLaw.DateFrom.ToString("dd.MM.yyyy") + ((caseLaw.DateTo != null) ? " до " + caseLaw.DateTo?.ToString("dd.MM.yyyy") : string.Empty) + ((caseLaw.DismisalLabelFull != string.Empty) ? " " + caseLaw.DismisalLabelFull : string.Empty);
            }
            var caseCourtId = repo.GetById<Case>(caseId).CourtId;
            int[] lawUnitsIds = caseLawUnitVMs.Select(c => c.LawUnitId).ToArray();
            var lawUnitsOrder = repo.AllReadonly<CourtLawUnitOrder>()
                                        .Where(x => x.CourtId == caseCourtId)
                                        .Where(x => lawUnitsIds.Contains(x.LawUnitId))
                                        .ToList();

            foreach (var caseLaw in caseLawUnitVMs)
            {
                if (caseLaw.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel)
                {
                    //Председателите излизат първи
                    caseLaw.OrderNumber = int.MinValue;
                    continue;
                }

                //Останалите лица в състава се редят по реда на старшинство
                var _order = lawUnitsOrder.FirstOrDefault(x => x.LawUnitId == caseLaw.LawUnitId);
                if (_order != null)
                {
                    caseLaw.OrderNumber = _order.OrderNumber;
                }
            }

            return caseLawUnitVMs.OrderBy(x => x.OrderNumber).AsQueryable();
        }

        public IQueryable<CaseLawUnitVM> CaseLawUnitByCaseFromSession_Select(int caseId)
        {
            var caseLawUnitVMs = repo.AllReadonly<CaseLawUnit>()
                                     .Include(x => x.LawUnit)
                                     .Include(x => x.JudgeRole)
                                     .Include(x => x.CaseSession)
                                     .Include(x => x.CourtDepartment)
                                     .Where(x => (x.CaseId == caseId) &&
                                                 (x.CaseSessionId != null) &&
                                                 (x.DateFrom <= x.CaseSession.DateFrom && (x.DateTo ?? x.CaseSession.DateFrom.AddDays(1)) >= x.CaseSession.DateFrom) &&
                                                 (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)))
                                     .Select(x => new CaseLawUnitVM()
                                     {
                                         Id = x.Id,
                                         CaseId = x.CaseId,
                                         CaseSessionId = x.CaseSessionId,
                                         LawUnitId = x.LawUnitId,
                                         JudgeRoleId = x.JudgeRoleId,
                                         JudgeRoleLabel = (x.JudgeRole != null) ? x.JudgeRole.Label : string.Empty,
                                         LawUnitName = (x.LawUnit != null) ? x.LawUnit.FullName : string.Empty,
                                         DepartmentLabel = (x.CourtDepartment != null) ? " " + x.CourtDepartment.Label : string.Empty
                                     }).ToList();

            return caseLawUnitVMs.AsQueryable();
        }

        public List<SelectListItem> CaseLawUnit_SelectForDropDownList(int caseId, int? caseSessionId)
        {
            var caseSessionNotificationLists = repo.AllReadonly<CaseSessionNotificationList>().Where(x => x.CaseSessionId == caseSessionId && x.DateExpired == null && x.NotificationPersonType == NomenclatureConstants.NotificationPersonType.CaseLawUnit).ToList();
            var date = ((caseSessionId ?? 0) == 0) ? DateTime.Now : repo.GetById<CaseSession>(caseSessionId).DateFrom;
            var result = repo.AllReadonly<CaseLawUnit>()
                             .Include(x => x.LawUnit)
                             .Where(x => x.CaseId == caseId && (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) &&
                                         x.LawUnit.LawUnitTypeId != NomenclatureConstants.LawUnitTypes.Judge &&
                                         (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                         (x.DateFrom <= date && (x.DateTo ?? date.AddDays(1)) >= date))
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.LawUnit.FullName + (caseSessionNotificationLists.Any(y => y.CaseLawUnitId == x.Id) ? ("(Призован номер " + caseSessionNotificationLists.Where(y => y.CaseLawUnitId == x.Id).FirstOrDefault().RowNumber + ")") : string.Empty)
                             })
                             .OrderBy(x => x.Text)
                             .ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        /// <summary>
        /// Добавяне на състав от дело към заседание
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseSessionId"></param>
        /// <returns></returns>
        public bool FillSessionLawUnitFromCase(int caseId, int caseSessionId)
        {
            var date = repo.GetById<CaseSession>(caseSessionId).DateFrom;
            var lawUnitsCase = repo.AllReadonly<CaseLawUnit>()
                                   .Include(x => x.LawUnit)
                                   .Where(x => x.CaseId == caseId &&
                                               (x.JudgeRoleId < NomenclatureConstants.JudgeRole.Jury) &&
                                               (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                               (x.DateFrom <= date && ((x.DateTo ?? date.AddDays(1)) >= date)))
                                   .ToList();

            var lawUnitsSession = repo.AllReadonly<CaseLawUnit>()
                                      .Include(x => x.LawUnit)
                                      .Where(x => x.CaseId == caseId &&
                                                  x.CaseSessionId == caseSessionId &&
                                                  (x.JudgeRoleId < NomenclatureConstants.JudgeRole.Jury) &&
                                                  (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)))
                                      .ToList();

            if (lawUnitsCase.Count > 0)
            {
                try
                {
                    foreach (var caseLawUnit in lawUnitsCase)
                    {
                        if (!lawUnitsSession.Any(x => x.LawUnitId == caseLawUnit.LawUnitId))
                        {
                            CaseLawUnit lawUnit = new CaseLawUnit()
                            {
                                CourtId = userContext.CourtId,
                                CaseId = caseId,
                                CaseSessionId = caseSessionId,
                                LawUnitId = caseLawUnit.LawUnitId,
                                JudgeRoleId = caseLawUnit.JudgeRoleId,
                                DateFrom = caseLawUnit.DateFrom,
                            };
                            repo.Add<CaseLawUnit>(lawUnit);
                        }
                    }
                    repo.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Грешка при запис на съдебен състав caseId={ caseId }   caseSessionId={ caseSessionId }");
                    return false;
                }
            }

            return true;
        }

        private IList<CheckListVM> FillCheckListVMs(int caseId, int caseSessionId, DateTime dateTime)
        {
            IList<CheckListVM> checkListVMs = new List<CheckListVM>();

            var caseSession = repo.GetById<CaseSession>(caseSessionId);
            var caseMeetings = repo.AllReadonly<CaseSessionMeeting>()
                                   .Where(x => x.CaseSessionId == caseSessionId && x.DateExpired == null)
                                   .ToList();

            var caseSessionMeetings = new List<CaseSessionMeeting>();

            foreach (var caseSessionMeeting in caseMeetings)
            {
                caseSessionMeetings.AddRange(repo.AllReadonly<CaseSessionMeeting>()
                                                 .Include(x => x.CaseSession)
                                                 .ThenInclude(x => x.CaseLawUnits)
                                                 .Include(x => x.Case)
                                                 .Where(x => (x.CaseSessionId != caseSessionId) &&
                                                             (x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno) &&
                                                             (x.CaseSession.DateExpired == null) &&
                                                             (x.DateExpired == null) &&
                                                             (((x.DateFrom <= caseSessionMeeting.DateFrom) && (caseSessionMeeting.DateTo <= x.DateTo)) ||
                                                             ((caseSessionMeeting.DateFrom <= x.DateTo) && (x.DateTo <= caseSessionMeeting.DateTo)) ||
                                                             ((caseSessionMeeting.DateFrom <= x.DateFrom) && (x.DateFrom <= caseSessionMeeting.DateTo)) ||
                                                             ((caseSessionMeeting.DateFrom <= x.DateFrom) && (x.DateTo <= caseSessionMeeting.DateTo))))
                                                 .ToList());
            }

            var lawUnitsCase = repo.AllReadonly<CaseLawUnit>()
                                   .Include(x => x.LawUnit)
                                   .Include(x => x.JudgeRole)
                                   .Where(x => x.CaseId == caseId &&
                                               x.CaseSessionId == null &&
                                               (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                               (x.DateFrom <= dateTime && ((x.DateTo ?? dateTime.AddDays(1)) >= dateTime)))
                                   .ToList();

            var lawUnitsSession = repo.AllReadonly<CaseLawUnit>()
                                      .Include(x => x.LawUnit)
                                      .Where(x => x.CaseId == caseId &&
                                                  x.CaseSessionId == caseSessionId &&
                                                  (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                                  (x.DateFrom <= dateTime && (x.DateTo ?? dateTime.AddDays(1)) >= dateTime))
                                      .ToList();

            foreach (var caseLawUnit in lawUnitsCase)
            {
                var isBusy = string.Empty;

                if (caseLawUnit.JudgeRoleId == NomenclatureConstants.JudgeRole.Jury)
                {
                    foreach (var caseSessionMeeting in caseSessionMeetings)
                    {
                        if (caseSessionMeeting.CaseSession.CaseLawUnits.Any(x => x.LawUnitId == caseLawUnit.LawUnitId))
                        {
                            if (isBusy == string.Empty)
                                isBusy = "Има застъпване с друго заседание/сесия по дело: " + caseSessionMeeting.Case.RegNumber + "/" + caseSessionMeeting.Case.RegDate.ToString("dd.MM.yyyy");
                            else
                                isBusy += ", " + caseSessionMeeting.Case.RegNumber + "/" + caseSessionMeeting.Case.RegDate.ToString("dd.MM.yyyy");

                        }
                    }
                }

                CheckListVM checkListVM = new CheckListVM
                {
                    Value = caseLawUnit.Id.ToString(),
                    Label = caseLawUnit.LawUnit.FullName + " - " + caseLawUnit.JudgeRole.Label,
                    Warrning = (isBusy != string.Empty) ? isBusy : string.Empty,
                    Checked = lawUnitsSession.Any(x => x.LawUnitId == caseLawUnit.LawUnitId)
                };
                checkListVMs.Add(checkListVM);
            }

            return checkListVMs.OrderBy(x => x.Label).ToList();
        }

        public CheckListViewVM CheckListViewVM_Fill(int caseId, int caseSessionId)
        {
            var caseCase = repo.GetById<Case>(caseId);
            var caseSession = repo.GetById<CaseSession>(caseSessionId);
            CheckListViewVM checkListViewVM = new CheckListViewVM
            {
                CourtId = caseId,
                ObjectId = caseSessionId,
                Label = "Изберете съдебен състав към дело " + caseCase.RegNumber + " и заседание ",
                checkListVMs = FillCheckListVMs(caseId, caseSessionId, caseSession.DateFrom)
            };

            return checkListViewVM;
        }

        /// <summary>
        /// Запис на състава от дело по заседание
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SessionLawUnitFromCase_SaveData(CheckListViewVM model)
        {
            var caseSession = repo.GetById<CaseSession>(model.ObjectId);
            var lawUnitsSession = repo.AllReadonly<CaseLawUnit>()
                                      .Include(x => x.LawUnit)
                                      .Where(x => x.CaseId == model.CourtId &&
                                                  x.CaseSessionId == model.ObjectId &&
                                                  (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)))
                                      .ToList();

            lawUnitsSession.Where(x => x.DateTo == null).ToList().ForEach(x => x.DateTo = caseSession.DateFrom.AddDays(-1));

            foreach (var check in model.checkListVMs)
            {
                var lawUnitCase = repo.GetById<CaseLawUnit>(int.Parse(check.Value));

                var units = lawUnitsSession.Where(x => x.LawUnitId == lawUnitCase.LawUnitId).FirstOrDefault();

                if (units != null)
                {
                    if (check.Checked)
                    {
                        units.DateTo = null;
                        units.JudgeRoleId = lawUnitCase.JudgeRoleId;
                        units.JudgeDepartmentRoleId = lawUnitCase.JudgeDepartmentRoleId;
                    }
                    else
                    {
                        if (units.DateTo == null)
                            units.DateTo = caseSession.DateFrom.AddDays(-1);
                    }
                }
                else
                {
                    if (check.Checked)
                    {
                        CaseLawUnit lawUnit = new CaseLawUnit()
                        {
                            CourtId = userContext.CourtId,
                            CaseId = model.CourtId,
                            CaseSessionId = model.ObjectId,
                            LawUnitId = lawUnitCase.LawUnitId,
                            LawUnitUserId = lawUnitCase.LawUnitUserId,
                            CourtDepartmentId = lawUnitCase.CourtDepartmentId,
                            JudgeRoleId = lawUnitCase.JudgeRoleId,
                            RealCourtDepartmentId = lawUnitCase.RealCourtDepartmentId,
                            CourtDutyId = lawUnitCase.CourtDutyId,
                            CourtGroupId = lawUnitCase.CourtGroupId,
                            JudgeDepartmentRoleId = lawUnitCase.JudgeDepartmentRoleId,
                            DateFrom = lawUnitCase.DateFrom,
                            UserId = userContext.UserId,
                            DateWrt = DateTime.Now
                        };

                        lawUnitsSession.Add(lawUnit);
                    }
                }
            }

            return SaveSessionLawUnit(lawUnitsSession);
        }

        public bool SaveSessionLawUnit(List<CaseLawUnit> caseLawUnits)
        {
            try
            {
                foreach (var unit in caseLawUnits)
                {
                    if (unit.Id > 0)
                        repo.Update(unit);
                    else
                        repo.Add<CaseLawUnit>(unit);
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                //logger.log(ex)
                return false;
            }
        }

        public List<SelectListItem> CaseLawUnit_OnlyJudge_SelectForDropDownList(int caseId, int? caseSessionId)
        {
            var date = ((caseSessionId ?? 0) == 0) ? DateTime.Now : repo.GetById<CaseSession>(caseSessionId).DateFrom;
            var result = repo.AllReadonly<CaseLawUnit>()
                             .Where(x => x.CaseId == caseId && (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) &&
                                         x.JudgeRoleId < NomenclatureConstants.JudgeRole.Jury &&
                                         (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                         (x.DateFrom <= date && ((x.DateTo ?? date.AddDays(1)) >= date)))
                             .OrderBy(x => x.JudgeRoleId)
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.LawUnit.FullName
                             })
                             .ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public CaseLawUnitDismisal CaseLawUnitDismisal_GetByCaseLawUnitId(int CaseLawUnitId)
        {
            return repo.AllReadonly<CaseLawUnitDismisal>()
                       .Include(x => x.DismisalType)
                       .Where(x => x.CaseLawUnitId == CaseLawUnitId)
                       .Where(CaseExtensions.ConfirmedDismissalsOnly())
                       .Select(x => x).FirstOrDefault();
        }

        /// <summary>
        /// Запис на отвод
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseLawUnitDismisal_SaveData(CaseLawUnitDismisal model)
        {
            model.CaseSessionActId = model.CaseSessionActId.EmptyToNull();
            if (model.DismisalTypeId != NomenclatureConstants.DismisalType.Otvod)
            {
                //Документ и лице искало отвода се избира само при Отвод
                model.DocumentId = null;
                model.DocumentPersonId = null;
                model.DismissalStateId = null;
            }
            bool removeSelectionCount = false;
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLawUnitDismisal>(model.Id);
                    saved.CaseSessionActId = model.CaseSessionActId;
                    saved.DismisalTypeId = model.DismisalTypeId;
                    saved.DismisalDate = model.DismisalDate;
                    saved.Description = model.Description;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;

                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseLawUnitDismisal>(model);
                    removeSelectionCount = true;
                }

                CaseLawUnit lawUnit = repo.GetById<CaseLawUnit>(model.CaseLawUnitId);
                if ((model.DismissalStateId ?? NomenclatureConstants.DismissalStates.Confirmed) == NomenclatureConstants.DismissalStates.Confirmed)
                {
                    //Реално се отвеждат само уважените!!! отводи, самоотводи и преразпределение
                    lawUnit.DateTo = model.DismisalDate;
                    lawUnit.DateWrt = DateTime.Now;
                    lawUnit.UserId = userContext.UserId;
                    //repo.Update(lawUnit);

                    var caseLawUnits = repo.All<CaseLawUnit>()
                                           .Include(x => x.CaseSession)
                                           .Where(x => x.LawUnitId == lawUnit.LawUnitId &&
                                                       x.CaseSessionId != null &&
                                                       x.CaseSession.DateFrom >= model.DismisalDate &&
                                                       x.CaseId == model.CaseId)
                                           .ToList() ?? new List<CaseLawUnit>();

                    foreach (var caseLaw in caseLawUnits)
                    {
                        caseLaw.DateTo = model.DismisalDate;
                        caseLaw.DateWrt = DateTime.Now;
                        caseLaw.UserId = userContext.UserId;
                        //repo.Update(caseLaw);
                    }
                }

                repo.SaveChanges();

                mqService.EPRO_AppendDismissal(model.Id, model.DismisalTypeId);

                if ((model.DismissalStateId ?? NomenclatureConstants.DismissalStates.Confirmed) == NomenclatureConstants.DismissalStates.Confirmed)
                {
                    if ((lawUnit.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter) && removeSelectionCount)
                    {
                        courtLoadPeriodService.UpdateDailyLoadPeriod_RemoveByDismisal(model.CaseLawUnitId);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на отвод Id={ model.Id }");
                return false;
            }
        }

        /// <summary>
        /// Извличане на данни за отводи
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public IQueryable<CaseLawUnitDismisalVM> CaseLawUnitDismisal_Select(int caseId)
        {
            return repo.AllReadonly<CaseLawUnitDismisal>()
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.ActType)
                       .Include(x => x.CaseSessionAct)
                       .ThenInclude(x => x.ActState)
                       .Include(x => x.DismisalType)
                       .Include(x => x.CaseLawUnit)
                       .ThenInclude(x => x.JudgeRole)
                       .Include(x => x.CaseLawUnit)
                       .ThenInclude(x => x.LawUnit)
                       .Where(x => x.CaseLawUnit.CaseId == caseId)
                       .Select(x => new CaseLawUnitDismisalVM()
                       {
                           Id = x.Id,
                           CaseLawUnitName = x.CaseLawUnit.LawUnit.FullName,
                           CaseLawUnitRole = x.CaseLawUnit.JudgeRole.Label,
                           CaseLawUnitRoleId = x.CaseLawUnit.JudgeRoleId,
                           CaseLawUnitAkt = x.CaseSessionAct.ActType.Label + " " + x.CaseSessionAct.ActState.Label + " " + (x.CaseSessionAct.RegNumber ?? string.Empty) + ((x.CaseSessionAct.RegDate != null) ? "/" + (x.CaseSessionAct.RegDate ?? DateTime.Now).ToString("dd.MM.yyyy") : string.Empty),
                           DismisalTypeLabel = x.DismisalType.Label + ((x.DismissalStateId == NomenclatureConstants.DismissalStates.Declined) ? " - Неуважен" : ""),
                           DismisalDate = x.DismisalDate,
                           Description = x.Description
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на данни за отводи за които няма ново разпределение
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<SelectListItem> CaseLawUnitFreeDismisal_SelectForDropDownList(int caseId, int roleId)
        {
            List<int> caseProtocol = repo.AllReadonly<CaseSelectionProtokol>()
                                   .Where(x => x.CaseId == caseId && x.CaseLawUnitDismisalId != null)

                                   .Select(x => (x.CaseLawUnitDismisalId ?? 0)).ToList();

            var result = repo.AllReadonly<CaseLawUnitDismisal>()
                             .Include(x => x.CaseLawUnit)
                             .Include(x => x.CaseLawUnit.LawUnit)
                             .Where(x => x.CaseLawUnit.CaseId == caseId && x.CaseLawUnit.JudgeRoleId == roleId && caseProtocol.Contains(x.Id) == false)
                             .Where(x => (NomenclatureConstants.DismisalType.DismisalList.Contains(x.DismisalTypeId)) || x.DismisalTypeId == NomenclatureConstants.DismisalType.Prerazpredelqne)
                             //Само по отвод за същата роля
                             .Where(x => x.CaseLawUnit.JudgeRoleId == roleId)
                             .Where(CaseExtensions.ConfirmedDismissalsOnly())
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.DismisalType.Label + ":" + x.CaseLawUnit.LawUnit.FullName
                             }).ToList();
            int caseAvailableRolesCount = repo.AllReadonly<CaseLawUnitCount>()
                                    .Where(x => x.CaseId == caseId)
                                    .Where(x => x.JudgeRoleId == roleId)
                                    .Select(x => x.PersonCount).FirstOrDefault();
            var dateTimeNow = DateTime.Now;
            int caseReservedRolesCount = repo.AllReadonly<CaseLawUnit>()
                                    .Where(x => x.CaseId == caseId)
                                    .Where(x => x.JudgeRoleId == roleId)
                                    .Where(x => (x.CaseSessionId ?? 0) == 0)
                                    .Where(x => x.DateFrom < dateTimeNow)
                                     .Where(x => (x.DateTo ?? dateTimeNow) >= dateTimeNow).Count();

            //Ако има само едно място за дадена роля и има отвод по тази роля не се дава възможност за избор без отвод
            if (((caseAvailableRolesCount - caseReservedRolesCount == 1) && (result.Count > 0)) == false)

            {
                result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            }
            return result;
        }

        public List<SelectListItem> GetJuryForSession_SelectForDropDownList(int caseSessionId)
        {
            var date = repo.GetById<CaseSession>(caseSessionId).DateFrom;
            var result = repo.AllReadonly<CaseLawUnit>()
                             .Include(x => x.LawUnit)
                             .Where(x => (x.CaseSessionId ?? 0) == caseSessionId &&
                                         x.LawUnit.LawUnitTypeId == NomenclatureConstants.LawUnitTypes.Jury &&
                                         (x.DateFrom <= date && (x.DateTo ?? date.AddDays(1)) >= date))
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.LawUnit.FullName
                             })
                             .OrderBy(x => x.Text)
                             .ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public List<SelectListItem> CaseLawUnitForCase_SelectForDropDownList(int caseId)
        {
            var result = repo.AllReadonly<CaseLawUnit>()
                             .Include(x => x.LawUnit)
                             .Where(x => x.CaseId == caseId &&
                                         x.CaseSessionId == null &&
                                         (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)))
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.LawUnit.FullName
                             })
                             .OrderBy(x => x.Text)
                             .ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public List<SelectListItem> CaseLawUnit_OnlyJudge_SelectForDropDownList_ValueLawUnitId(int caseId, int? caseSessionId)
        {
            var date = ((caseSessionId ?? 0) == 0) ? DateTime.Now : repo.GetById<CaseSession>(caseSessionId).DateFrom;
            var result = repo.AllReadonly<CaseLawUnit>()
                             .Include(x => x.JudgeRole)
                             .Where(x => x.CaseId == caseId && (x.CaseSessionId ?? 0) == (caseSessionId ?? 0) &&
                                         x.JudgeRoleId < NomenclatureConstants.JudgeRole.Jury &&
                                         (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                         (x.DateFrom <= date && ((x.DateTo ?? date.AddDays(1)) >= date)))
                             .OrderBy(x => x.JudgeRoleId)
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.LawUnitId.ToString(),
                                 Text = x.LawUnit.FullName + " - " + x.JudgeRole.Label
                             })
                             .ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public bool IsFullComposition(int CaseId)
        {
            var countComposition = repo.AllReadonly<CaseLawUnitCount>()
                                       .Where(x => x.CaseId == CaseId)
                                       .Sum(x => x.PersonCount);
            var dtNow = DateTime.Now;
            var countCaseLawUnit = repo.AllReadonly<CaseLawUnit>()
                                       .Where(x => (x.CaseId == CaseId) &&
                                                   (x.CaseSessionId == null) &&
                                                   (!NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                                   ((x.DateFrom <= dtNow) && ((x.DateTo ?? DateTime.MaxValue) >= dtNow)))
                                       .Count();

            return countComposition == countCaseLawUnit;
        }

        public bool CaseLawUnit_SaveData(CaseLawUnit model)
        {
            model.CaseSessionId = model.CaseSessionId.EmptyToNull();
            model.CourtDepartmentId = model.CourtDepartmentId.EmptyToNull();
            model.CourtDutyId = model.CourtDutyId.EmptyToNull();
            model.CourtGroupId = model.CourtGroupId.EmptyToNull();
            model.JudgeDepartmentRoleId = model.JudgeDepartmentRoleId.EmptyToNull();
            model.LawUnitUserId = GetUserIdByLawUnitId(model.LawUnitId);
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseLawUnit>(model.Id);
                    saved.LawUnitId = model.LawUnitId;
                    saved.LawUnitUserId = model.LawUnitUserId;
                    saved.JudgeRoleId = model.JudgeRoleId;
                    saved.DateFrom = model.DateFrom;
                    saved.DateTo = model.DateTo;
                    saved.Description = model.Description;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;
                    repo.Update(saved);
                }
                else
                {
                    //Insert
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseLawUnit>(model);
                }

                if (model.DateTo != null && model.CaseSessionId == null)
                {
                    var caseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                           .Where(x => x.CaseId == model.CaseId &&
                                                       x.CaseSessionId != null &&
                                                       x.LawUnitId == model.LawUnitId &&
                                                       x.JudgeRoleId == model.JudgeRoleId)
                                           .ToList();

                    foreach (var caseLaw in caseLawUnits)
                    {
                        caseLaw.DateTo = model.DateTo;
                        caseLaw.DateWrt = DateTime.Now;
                        caseLaw.UserId = userContext.UserId;
                        repo.Update(caseLaw);
                    }

                    if (model.JudgeRoleId == NomenclatureConstants.JudgeRole.Secretary)
                    {
                        var caseSessionMeetingUsers = repo.AllReadonly<CaseSessionMeetingUser>()
                                                          .Where(x => x.CaseId == model.CaseId &&
                                                                      x.SecretaryUserId == model.LawUnitUserId &&
                                                                      x.CaseSessionMeeting.DateFrom >= model.DateTo)
                                                          .ToList();

                        repo.DeleteRange(caseSessionMeetingUsers);
                    }
                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на лице от състава Id={ model.Id }");
                return false;
            }
        }

        public bool CaseLawUnit_RefreshData(int CaseId, int CaseSessionId)
        {
            try
            {
                var caseSession = repo.GetById<CaseSession>(CaseSessionId);
                var dateNowAdd100Year = DateTime.Now.AddYears(100);

                var CaseCaseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                           .Where(x => x.CaseId == CaseId &&
                                                       x.CaseSessionId == null &&
                                                       NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId) &&
                                                       (x.DateTo ?? caseSession.DateFrom.AddYears(100)) >= caseSession.DateFrom)
                                           .ToList();

                var SessionCaseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                              .Where(x => x.CaseId == CaseId &&
                                                          x.CaseSessionId == CaseSessionId &&
                                                          NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId))
                                              .ToList();

                foreach (var caseLaw in CaseCaseLawUnits)
                {
                    var caseLawUnit = SessionCaseLawUnits.Where(x => x.LawUnitId == caseLaw.LawUnitId && x.JudgeRoleId == caseLaw.JudgeRoleId).OrderByDescending(x => x.Id).FirstOrDefault();

                    if (caseLawUnit == null)
                    {
                        if ((caseLaw.DateFrom < caseSession.DateTo) && ((caseLaw.DateTo ?? DateTime.Now.AddYears(100)) >= caseSession.DateFrom))
                        {
                            caseLaw.Id = 0;
                            caseLaw.CaseSessionId = CaseSessionId;
                            caseLaw.DateWrt = DateTime.Now;
                            caseLaw.UserId = userContext.UserId;
                            repo.Add<CaseLawUnit>(caseLaw);
                        }
                    }
                    else
                    {
                        caseLawUnit.DateFrom = caseLaw.DateFrom;
                        caseLawUnit.DateTo = caseLaw.DateTo;
                        caseLawUnit.DateWrt = DateTime.Now;
                        caseLawUnit.UserId = userContext.UserId;
                        repo.Update<CaseLawUnit>(caseLawUnit);
                    }
                }

                foreach (var lawUnit in SessionCaseLawUnits)
                {
                    var caseLawUnit = CaseCaseLawUnits.Where(x => x.LawUnitId == lawUnit.LawUnitId && x.JudgeRoleId == lawUnit.JudgeRoleId).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (caseLawUnit == null)
                    {
                        lawUnit.DateTo = caseSession.DateFrom.AddMinutes(-1);
                        lawUnit.DateWrt = DateTime.Now;
                        lawUnit.UserId = userContext.UserId;
                        repo.Update<CaseLawUnit>(lawUnit);
                    }

                }

                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис при опресняване на данните в заседание Id={ CaseSessionId }");
                return false;
            }
        }

        public List<CaseLawUnit> GetJudgeFromCase(int caseId, int? caseSessionId = null)
        {
            return repo.AllReadonly<CaseLawUnit>()
                       .Include(x => x.JudgeRole)
                       .Include(x => x.JudgeDepartmentRole)
                       .Include(x => x.LawUnit)
                       .Include(x => x.RealCourtDepartment)
                       .Include(x => x.CourtDepartment)
                       .Where(x => x.CaseId == caseId &&
                                   x.CaseSessionId == caseSessionId &&
                                   (NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) &&
                                   (x.DateFrom <= DateTime.Now && (x.DateTo ?? DateTime.Now.AddYears(100)) >= DateTime.Now))
                       .ToList() ?? new List<CaseLawUnit>();
        }

        private List<CaseLawUnit> GetJudgeCaseLawUnitFromCase(int caseId, int? CaseSessionId = null)
        {
            var dateTimeNow = DateTime.Now;
            return repo.All<CaseLawUnit>()
                       .Where(x => (x.CaseId == caseId) &&
                                   (CaseSessionId == null ? x.CaseSessionId == null : x.CaseSessionId == CaseSessionId) &&
                                   (NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) &&
                                   (x.DateFrom <= dateTimeNow && (x.DateTo ?? dateTimeNow.AddYears(100)) >= dateTimeNow))
                       .ToList() ?? new List<CaseLawUnit>();
        }

        private List<CaseLawUnit> GetJudgeCaseLawUnitFromCaseSessionAfterDate(int caseId)
        {
            var dateTimeNow = DateTime.Now;
            return repo.All<CaseLawUnit>()
                       .Where(x => (x.CaseId == caseId) &&
                                   (x.CaseSessionId != null) &&
                                   (x.CaseSession.DateFrom >= DateTime.Now) &&
                                   (x.CaseSession.DateExpired == null) &&
                                   (NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) &&
                                   (x.DateFrom <= dateTimeNow && (x.DateTo ?? dateTimeNow.AddYears(100)) >= dateTimeNow))
                       .ToList() ?? new List<CaseLawUnit>();
        }

        public List<SelectListItem> GetDDL_GetJudgeFromCase(int caseId, int? caseSessionId = null)
        {
            var selectListItems = GetJudgeFromCase(caseId, caseSessionId).Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.LawUnit.FullName +
                                                                      (x.JudgeRole != null ? " - " + x.JudgeRole.Label : string.Empty) +
                                                                      (x.JudgeDepartmentRole != null ? ", роля в състава: " + x.JudgeDepartmentRole.Label : string.Empty)
            })
                                                          .ToList() ?? new List<SelectListItem>();

            return selectListItems;
        }

        private IQueryable<CourtDepartmentVM> CourtDepartmentByLawUnit_Select(int LawUnitId, int CourtId)
        {
            return repo.AllReadonly<CourtDepartmentLawUnit>()
                       .Where(x => x.CourtDepartment.CourtId == CourtId &&
                                   x.LawUnitId == LawUnitId &&
                                   x.CourtDepartment.DepartmentTypeId == NomenclatureConstants.DepartmentType.Systav &&
                                   (x.DateFrom <= DateTime.Now && ((x.DateTo != null) ? x.DateTo >= DateTime.Now : true)))
                       .Select(x => new CourtDepartmentVM()
                       {
                           Id = x.CourtDepartment.Id,
                           Label = x.CourtDepartment.Label,
                           CourtLabel = x.CourtDepartment.Court.Label,
                           DepartmentTypeLabel = (x.CourtDepartment.DepartmentType != null) ? x.CourtDepartment.DepartmentType.Label : string.Empty,
                           MasterId = x.CourtDepartment.MasterId,
                           ParentId = (x.CourtDepartment.ParentId ?? 0)
                       })
                       .AsQueryable();
        }

        public List<SelectListItem> GetDDL_GetListDepartmentFromRealDepartment(int caseId)
        {
            var caseLawUnits = GetJudgeFromCase(caseId).ToList();

            var selectListItems = new List<SelectListItem>();

            foreach (var caseLaw in caseLawUnits.Where(x => x.RealCourtDepartmentId != null))
            {
                if (!selectListItems.Any(x => x.Value == caseLaw.RealCourtDepartmentId.ToString()))
                {
                    selectListItems.Add(new SelectListItem()
                    {
                        Value = caseLaw.RealCourtDepartmentId.ToString(),
                        Text = caseLaw.RealCourtDepartment.Label
                    });
                }
            }

            foreach (var caseLaw in caseLawUnits.Where(x => x.CourtDepartmentId != null))
            {
                if (!selectListItems.Any(x => x.Value == caseLaw.CourtDepartmentId.ToString()))
                {
                    selectListItems.Add(new SelectListItem()
                    {
                        Value = caseLaw.CourtDepartmentId.ToString(),
                        Text = caseLaw.CourtDepartment.Label
                    });
                }
            }

            foreach (var caseLaw in caseLawUnits)
            {
                var courtDepartments = CourtDepartmentByLawUnit_Select(caseLaw.LawUnitId, userContext.CourtId);
                foreach (var courtDepartment in courtDepartments)
                {
                    if (!selectListItems.Any(x => x.Value == courtDepartment.Id.ToString()))
                    {
                        selectListItems.Add(new SelectListItem()
                        {
                            Value = courtDepartment.Id.ToString(),
                            Text = courtDepartment.Label
                        });
                    }
                }
            }

            //Ако съществува протокол за СД без състав се добавя и възможност за премахване на състава
            var hasProtokolsWithNoCompartment = repo.AllReadonly<CaseSelectionProtokol>()
                            .Where(x => x.CaseId == caseId)
                            .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter && x.CompartmentID == null)
                            .Where(x => x.SelectionProtokolStateId == NomenclatureConstants.SelectionProtokolState.Signed)
                            .Any();

            if (hasProtokolsWithNoCompartment)
            {
                selectListItems.Add(new SelectListItem()
                {
                    Value = "-1",
                    Text = "Без избран състав"
                });
            }

            return selectListItems;
        }

        public CaseLawUnitChangeDepRolVM GetCaseLawUnitChangeDepRol(int caseId, int? caseSessionId = null)
        {
            var caseLaw = GetJudgeFromCase(caseId, caseSessionId).Where(x => x.JudgeDepartmentRoleId == NomenclatureConstants.JudgeDepartmentRole.Predsedatel).FirstOrDefault();
            return new CaseLawUnitChangeDepRolVM()
            {
                CaseId = caseId,
                CaseSessionId = caseSessionId,
                CaseLawUnitId = (caseLaw != null) ? caseLaw.Id : -1,
                DepartmentId = (caseLaw != null) ? caseLaw.CourtDepartmentId : -1
            };
        }

        private List<CaseSession> GetCaseSessionsAfterDate(int CaseId, DateTime dateTime)
        {
            return repo.AllReadonly<CaseSession>()
                       .Where(x => x.CaseId == CaseId &&
                                   x.DateExpired == null &&
                                   x.DateFrom >= dateTime)
                       .ToList() ?? new List<CaseSession>();
        }

        public bool GetCaseLawUnitChangeDepRol_Save(CaseLawUnitChangeDepRolVM model)
        {
            bool removeDepartment = model.DepartmentId == -1;
            model.CaseLawUnitId = model.CaseLawUnitId.EmptyToNull();
            model.DepartmentId = model.DepartmentId.EmptyToNull();

            try
            {
                if ((model.CaseLawUnitId != null) || (model.DepartmentId != null))
                {
                    var caseLawUnits = GetJudgeCaseLawUnitFromCase(model.CaseId, model.CaseSessionId);
                    var caseLawUnitSessions = GetJudgeCaseLawUnitFromCaseSessionAfterDate(model.CaseId);

                    foreach (var caseLaw in caseLawUnits)
                    {
                        if (caseLaw.Id == (model.CaseLawUnitId ?? 0))
                        {
                            caseLaw.JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Predsedatel;

                            if (model.DepartmentId != null)
                                caseLaw.CourtDepartmentId = model.DepartmentId;
                            if (removeDepartment)
                            {
                                caseLaw.CourtDepartmentId = null;
                            }
                        }
                        else
                        {
                            caseLaw.JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Member;

                            if (model.DepartmentId != null)
                                caseLaw.CourtDepartmentId = model.DepartmentId;

                            if (removeDepartment)
                            {
                                caseLaw.CourtDepartmentId = null;
                            }
                        }

                        caseLaw.DateWrt = DateTime.Now;
                        caseLaw.UserId = userContext.UserId;
                        //repo.Update(caseLaw);

                        if (model.CaseSessionId == null)
                        {
                            foreach (var caseLawSession in caseLawUnitSessions.Where(x => x.LawUnitId == caseLaw.LawUnitId))
                            {
                                caseLawSession.JudgeDepartmentRoleId = caseLaw.JudgeDepartmentRoleId;
                                caseLawSession.CourtDepartmentId = caseLaw.CourtDepartmentId;
                                caseLawSession.DateWrt = DateTime.Now;
                                caseLawSession.UserId = userContext.UserId;
                                //repo.Update(caseLawSession);
                            }
                        }
                    }
                }

                repo.SaveChanges();
                commonService.UpdateCaseJudicalCompositionOtdelenie(model.CaseId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на промяна на председател/състав по дело с Id={ model.CaseId }");
                return false;
            }
        }

        public List<SelectListItem> CaseLawUnitForCaseObligation_SelectForDropDownList(int caseId)
        {
            var result = repo.AllReadonly<CaseLawUnit>()
                             .Include(x => x.LawUnit)
                             .Where(x => x.CaseId == caseId &&
                                         x.CaseSessionId == null &&
                                         (NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.JudgeRoleId)) == false)
                             .Select(x => new SelectListItem()
                             {
                                 Value = x.Id.ToString(),
                                 Text = x.LawUnit.FullName
                             })
                             .OrderBy(x => x.Text)
                             .ToList();

            result.Insert(0, new SelectListItem() { Text = "Избери", Value = "-1" });
            return result;
        }

        public bool IsExistLawUnitByCase(int CaseId, DateTime DateFrom)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            return repo.AllReadonly<CaseLawUnit>()
                       .Any(x => (x.CaseId == CaseId) &&
                                 (x.CaseSessionId == null) &&
                                 (NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) &&
                                 ((x.DateFrom <= DateFrom) && ((x.DateTo ?? dateEnd) >= DateFrom)));
        }

        public bool IsExistJudgeReporterByCase(int CaseId, DateTime DateFrom)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            return repo.AllReadonly<CaseLawUnit>()
                       .Any(x => (x.CaseId == CaseId) &&
                                 (x.CaseSessionId == null) &&
                                 (x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter) &&
                                 ((x.DateFrom <= DateFrom) && ((x.DateTo ?? dateEnd) >= DateFrom)));
        }

        public bool IsExistManualLawUnitByCase(int CaseId, int LawUnitId, DateTime DateFrom)
        {
            DateTime dateEnd = DateTime.Now.AddYears(100);
            return repo.AllReadonly<CaseLawUnit>()
                       .Any(x => (x.CaseId == CaseId) &&
                                 (x.CaseSessionId == null) &&
                                 (x.LawUnitId == LawUnitId) &&
                                 (NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)) &&
                                 ((x.DateFrom <= DateFrom) && ((x.DateTo ?? dateEnd) >= DateFrom)));
        }

        public IQueryable<CourtLawUnitSubstitutionVM> LawUnitSubstitution_SelectForSession(int caseSessionId)
        {
            var caseSession = repo.GetById<CaseSession>(caseSessionId);
            if (caseSession == null)
            {
                return null;
            }
            var sessionLawUnits = CaseLawUnit_Select(caseSession.CaseId, caseSession.Id);
            int[] sessionLawUnitIds = sessionLawUnits.Select(x => x.SubstitutedLawUnitId).ToArray();
            var substitutionsAvailable = repo.AllReadonly<CourtLawUnitSubstitution>()
                                            .Where(x => x.CourtId == caseSession.CourtId)
                                            .Where(x => sessionLawUnitIds.Contains(x.LawUnitId))
                                            .Where(x => !sessionLawUnitIds.Contains(x.SubstituteLawUnitId))
                                            .Where(x => x.DateFrom <= caseSession.DateTo && x.DateTo >= caseSession.DateFrom)
                                            .Where(FilterExpireInfo<CourtLawUnitSubstitution>(false))
                                            .ProjectTo<CourtLawUnitSubstitutionVM>(CourtLawUnitSubstitutionVM.GetMapping())
                                            .ToList();

            foreach (var item in substitutionsAvailable)
            {
                if (sessionLawUnits.Any(x => x.SubstitutionId == item.Id))
                {
                    item.IsSubstituted = true;
                }
            }

            return substitutionsAvailable.AsQueryable();
        }

        public bool LawUnitSubstitution_Apply(int substsitution_id, int from, int to, int caseSessionId)
        {
            var caseSession = repo.GetById<CaseSession>(caseSessionId);
            if (caseSession == null)
            {
                return false;
            }

            var substitution = repo.GetById<CourtLawUnitSubstitution>(substsitution_id);
            if (substitution == null)
            {
                return false;
            }

            bool isApply = substitution.LawUnitId == from && substitution.SubstituteLawUnitId == to;
            bool isRevert = substitution.LawUnitId == to && substitution.SubstituteLawUnitId == from;
            if ((isApply || isRevert) == false)
            {
                return false;
            }

            var sessionLawUnits = CaseLawUnit_Select(caseSession.CaseId, caseSession.Id);
            var judgeToReplace = sessionLawUnits.Where(x => x.LawUnitId == from).FirstOrDefault();
            if (judgeToReplace == null)
            {
                return false;
            }

            //Ако е необходимо да се замести титуляра от разпределението със съдия по заместване
            if (isApply)
            {
                var lawUnit = repo.GetById<CaseLawUnit>(judgeToReplace.Id);
                lawUnit.LawUnitId = substitution.SubstituteLawUnitId;
                lawUnit.LawUnitSubstitutionId = substitution.Id;
                lawUnit.LawUnitUserId = GetUserIdByLawUnitId(lawUnit.LawUnitId);
                repo.Update(lawUnit);
                repo.SaveChanges();
                return true;
            }
            //Ако е необходимо да се възстанови титуляра
            if (isRevert)
            {

            }

            return false;
        }

        public IQueryable<CaseLawUnitManualJudgeVM> LawUnitManualJudge_Select(int? id, DateTime? dateFrom, DateTime? dateTo, string caseNumber, string lawunitName)
        {
            int courtId = userContext.CourtId;
            return repo.AllReadonly<CaseLawUnitManualJudge>()
                       .Include(x => x.Case)
                       .Include(x => x.LawUnit)
                       .Include(x => x.JudgeRole)
                       .Include(x => x.User)
                       .ThenInclude(x => x.LawUnit)
                       .Where(x => x.Id == (id ?? x.Id))
                       .Where(x => x.CourtId == userContext.CourtId)
                       .Where(x =>
                          EF.Functions.ILike(x.Case.RegNumber, caseNumber.ToCasePaternSearch())
                          && x.DateWrt >= (dateFrom ?? DateTime.MinValue) && x.DateWrt <= (dateTo ?? DateTime.MaxValue)
                       )
                       .Where(x => EF.Functions.ILike(x.LawUnit.FullName, lawunitName.ToPaternSearch()))
                       .Select(x => new CaseLawUnitManualJudgeVM
                       {
                           Id = x.Id,
                           CaseId = x.CaseId,
                           CaseNumber = x.Case.RegNumber,
                           LawUnitName = x.LawUnit.FullName,
                           JudgeRoleName = x.JudgeRole.Label,
                           ChangeDate = x.DateWrt,
                           Description = x.Description,
                           ChangeUserName = x.User.LawUnit.FullName
                       }).AsQueryable();
        }

        public SaveResultVM LawUnitManualJudge_SaveData(CaseLawUnitManualJudge model)
        {
            SaveResultVM result = new SaveResultVM();
            DateTime dtNow = DateTime.Now;

            var caseLawUnits = repo.AllReadonly<CaseLawUnit>()
                                        .Where(x => x.CaseId == model.CaseId && x.CaseSessionId == null)
                                        .ToList();

            if (!caseLawUnits.Any())
            {
                result.ErrorMessage = "В избраното дело няма разпределени съдии.";
            }

            var hasActiveCaseLawUnits = caseLawUnits.Where(x => NomenclatureConstants.JudgeRole.JudgeRolesList.Contains(x.JudgeRoleId) && (x.DateTo ?? DateTime.MaxValue) > dtNow).Any();

            if (hasActiveCaseLawUnits)
            {
                result.ErrorMessage = "В избраното дело има съдии, които не са отведени.";
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return result;
            }

            try
            {
                model.CourtId = userContext.CourtId;
                SetUserDateWRT(model);

                //Добавяне на нов ръчно избран съдия при масов отвод
                var newCaseLawunit = new CaseLawUnit()
                {
                    CourtId = userContext.CourtId,
                    CaseId = model.CaseId,
                    CaseSessionId = null,
                    LawUnitId = model.LawUnitId,
                    LawUnitUserId = GetUserIdByLawUnitId(model.LawUnitId),
                    JudgeDepartmentRoleId = NomenclatureConstants.JudgeDepartmentRole.Predsedatel,
                    JudgeRoleId = model.JudgeRoleId,
                    Description = model.Description,
                    DateFrom = model.DateFrom
                };

                SetUserDateWRT(newCaseLawunit);


                repo.Add(newCaseLawunit);

                repo.Add(model);

                repo.SaveChanges();

                result.Result = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис");
                result.Result = false;
            }

            return result;
        }

        public CaseLawUnit GetJudgeReporter(int caseId)
        {
            DateTime dateEnd = DateTime.Now.AddDays(1);
            return repo.AllReadonly<CaseLawUnit>()
                         .Where(x => x.CaseId == caseId)
                         .Where(x => x.CaseSessionId == null)
                         .Where(x => (x.DateTo ?? dateEnd) >= DateTime.Now)
                         .Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                         .FirstOrDefault();
        }

        public List<SelectListItem> GetDDL_LeftSide(int CaseId, bool addDefaultElement = true)
        {
            var selectListItems = repo.AllReadonly<CasePerson>()
                                      .Include(x => x.PersonRole)
                                      .Where(x => x.CaseId == CaseId &&
                                                  x.CaseSessionId == null &&
                                                  x.DateExpired == null &&
                                                  x.PersonRole.RoleKindId == NomenclatureConstants.PersonKinds.LeftSide &&
                                                  ((x.DateTo ?? DateTime.Now.AddYears(1)) >= DateTime.Now))
                                      .Select(x => new SelectListItem()
                                      {
                                          Text = x.FullName,
                                          Value = x.Id.ToString()
                                      })
                                      .OrderBy(x => x.Text)
                                      .ToList() ?? new List<SelectListItem>();

            if (addDefaultElement)
            {
                selectListItems = selectListItems.Prepend(new SelectListItem() { Text = "Избери", Value = "-1" })
                                                 .ToList();
            }

            return selectListItems;
        }

        public List<CheckListVM> GetCheckListCaseLawUnitByCase(int caseId)
        {
            DateTime dateFrom = DateTime.Now;
            DateTime dateEnd = DateTime.Now.AddYears(100);
            var lawUnits = repo.AllReadonly<CaseLawUnit>()
                               .Include(x => x.LawUnit)
                               .Include(x => x.JudgeRole)
                               .Include(x => x.JudgeDepartmentRole)
                               .Where(x => (x.CaseId == caseId) &&
                                           (x.CaseSessionId == null) &&
                                           (NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) &&
                                           (((x.DateTo ?? dateEnd) >= dateFrom))).ToList();

            var result = new List<CheckListVM>();

            if (lawUnits.Count > 1)
            {
                foreach (var caseLaw in lawUnits)
                {
                    var checkElement = new CheckListVM()
                    {
                        Checked = caseLaw.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter,
                        Label = caseLaw.LawUnit.FullName + " (" + caseLaw.JudgeRole.Label + (caseLaw.JudgeDepartmentRole != null ? "/" + caseLaw.JudgeDepartmentRole.Label : string.Empty) + ")",
                        Value = caseLaw.Id.ToString()
                    };

                    result.Add(checkElement);
                }
            }

            return result;
        }

        public List<CheckListVM> GetCheckListCaseLawUnitByCaseAll(int caseId)
        {
            DateTime dateFrom = DateTime.Now;
            DateTime dateEnd = DateTime.Now.AddYears(100);
            var lawUnits = repo.AllReadonly<CaseLawUnit>()
                               .Include(x => x.LawUnit)
                               .Include(x => x.JudgeRole)
                               .Include(x => x.JudgeDepartmentRole)
                               .Where(x => (x.CaseId == caseId) &&
                                           (x.CaseSessionId == null) &&
                                           (NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) &&
                                           (((x.DateTo ?? dateEnd) >= dateFrom))).ToList();

            var result = new List<CheckListVM>();

            foreach (var caseLaw in lawUnits)
            {
                var checkElement = new CheckListVM()
                {
                    Checked = caseLaw.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter,
                    Label = caseLaw.LawUnit.FullName + " (" + caseLaw.JudgeRole.Label + (caseLaw.JudgeDepartmentRole != null ? "/" + caseLaw.JudgeDepartmentRole.Label : string.Empty) + ")",
                    Value = caseLaw.Id.ToString()
                };

                result.Add(checkElement);
            }

            return result;
        }

        public bool IsExistJudgeLawUnitInCase(int CaseId)
        {
            var lawUnitId = userContext.LawUnitId;
            var lawUnit = repo.GetById<LawUnit>(lawUnitId);
            if (lawUnit.LawUnitTypeId != NomenclatureConstants.LawUnitTypes.Judge)
                return true;

            DateTime dateFrom = DateTime.Now;
            DateTime dateEnd = DateTime.Now.AddYears(100);
            return repo.AllReadonly<CaseLawUnit>()
                       .Any(x => (x.CaseId == CaseId) &&
                                 (x.CaseSessionId == null) &&
                                 (x.LawUnitId == lawUnitId) &&
                                 (NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(x.JudgeRoleId)) &&
                                 (((x.DateTo ?? dateEnd) >= dateFrom)));
        }

        public bool IsExistIsExistManualLawUnitInConductedSession(int caseId, DateTime dateTo, int lawUnitId)
        {
            var dateNow = DateTime.Now;
            return repo.AllReadonly<CaseSession>()
                       .Any(x => x.CaseId == caseId &&
                                 x.DateExpired == null &&
                                 x.SessionStateId == NomenclatureConstants.SessionState.Provedeno &&
                                 x.DateFrom >= dateTo &&
                                 x.CaseLawUnits.Any(l => l.LawUnitId == lawUnitId &&
                                                         (l.DateTo ?? dateNow.AddYears(100)) >= dateTo));
        }
    }
}
