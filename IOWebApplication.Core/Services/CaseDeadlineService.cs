using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CaseDeadlineService : BaseService, ICaseDeadlineService
    {
        private readonly IUrlHelper urlHelper;
        private readonly IWorkNotificationService workNotificationService;
        private readonly IWorkingDaysService workingDaysService;
        private readonly INomenclatureService nomService;

        public CaseDeadlineService(ILogger<CaseDeadlineService> _logger,
                                   IRepository _repo,
                                   IUserContext _userContext,
                                   IUrlHelper _urlHelper,
                                   IWorkNotificationService _workNotificationService,
                                   IWorkingDaysService _workingDaysService,
                                   INomenclatureService _nomService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            urlHelper = _urlHelper;
            workNotificationService = _workNotificationService;
            workingDaysService = _workingDaysService;
            nomService = _nomService;
        }


        public void DeadLineOnSessionResult(CaseSessionResult sessionResult)
        {
            DeadLineDeclaredForResolve(sessionResult);
            // Вече се гаси от подписване на протокол
            // DeadLineOpenSessionResultComplete(sessionResult);
            // DeadLineDeclaredForResolveCompleteOnResult(sessionResult);
            DeadLineCompanyCaseCompleteOnResult(sessionResult);
        }

        public void DeadLineOnCase(Case caseModel)
        {
            DeadLineCompanyCase(caseModel);
        }

        public void DeadLineOnSession(CaseSession session)
        {
            DeadLineOpenSessionResult(session);
            if (session.DateExpired != null)
            {
                repo.SaveChanges();
                var deadlines = repo.AllReadonly<CaseDeadline>()
                         .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                     x.SourceId == session.Id &&
                                     x.DateExpired == null)
                         .ToList();
                SaveDeadLineExpired(deadlines);
                repo.SaveChanges();
                var caseSessionActIds = repo.AllReadonly<CaseSessionAct>()
                                         .Where(x => x.CaseSessionId == session.Id)
                                         .Select(x => x.Id)
                                         .ToArray()
                                         .Select(x => (long)x)
                                         .ToArray();
                deadlines = repo.AllReadonly<CaseDeadline>()
                       .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct &&
                                   caseSessionActIds.Contains(x.SourceId) &&
                                   x.DateExpired == null)
                       .ToList();
                SaveDeadLineExpired(deadlines);
            }
        }

        public void DeadLineCompleteOnSessionAct(CaseSessionAct caseSessionAct)
        {
            DeadLineDeclaredForResolveComplete(caseSessionAct);
            DeadLineOpenSessionResultCompleteOnAct(caseSessionAct);
        }

        private bool sessionHaveResult(CaseSession session, int resultId)
        {
            var caseSessionResults = session.CaseSessionResults;
            if (caseSessionResults == null || !caseSessionResults.Any())
                caseSessionResults = repo.AllReadonly<CaseSessionResult>()
                                         .Where(x => x.IsActive && x.DateExpired == null)
                                         .Where(x => x.CaseSessionId == session.Id)
                                         .ToList();
            return caseSessionResults.Any(x => x.SessionResultId == resultId);
        }

        private void setUnExpired(CaseDeadline deadline)
        {
            deadline.DateExpired = null;
            deadline.UserExpiredId = null;
            deadline.DescriptionExpired = "";
        }

        private void setExpired(CaseDeadline deadline)
        {
            deadline.DateExpired = DateTime.Now;
            deadline.UserExpiredId = ImpersonatedUserId ?? userContext.UserId;
            deadline.DescriptionExpired = "";
        }

        private void setDateEnd(CaseDeadline deadline, Infrastructure.Data.Models.Nomenclatures.DeadlineType deadlineType, bool isSpecial = false)
        {
            int? months = isSpecial ? deadlineType.DeadlineSpecialMonths : deadlineType.DeadlineMonths;
            if (months != null)
            {
                int month = months ?? 0;
                deadline.EndDate = deadline.StartDate.AddMonths(month).Date;
                while (!workingDaysService.IsWorkingDay(ImpersonatedCourtId ?? userContext.CourtId, deadline.EndDate))
                {
                    deadline.EndDate = deadline.EndDate.AddDays(1).Date;
                }
            }

            int? workingDays = isSpecial ? deadlineType.DeadlineSpecialWorkingDays : deadlineType.DeadlineWorkingDays;
            if (workingDays != null)
            {
                deadline.EndDate = deadline.StartDate.Date;
                int wDays = (workingDays ?? 0) - 1;
                while (wDays > 0)
                {
                    deadline.EndDate = deadline.EndDate.AddDays(1);
                    if (workingDaysService.IsWorkingDay(ImpersonatedCourtId ?? userContext.CourtId, deadline.EndDate))
                        wDays--;
                }
            }

            int? normalDays = isSpecial ? deadlineType.DeadlineSpecialDays : deadlineType.DeadlineDays;
            if (normalDays != null)
            {
                int days = normalDays ?? 0;
                deadline.EndDate = deadline.StartDate.AddDays(days - 1).Date;
                while (!workingDaysService.IsWorkingDay(ImpersonatedCourtId ?? userContext.CourtId, deadline.EndDate))
                {
                    deadline.EndDate = deadline.EndDate.AddDays(1).Date;
                }
            }
        }

        private void ExpireWorkNotifications(CaseDeadline deadline)
        {
            var workNotifications = repo.All<WorkNotification>()
                                 .Where(x => x.CaseDeadlineId == deadline.Id && x.DateExpired == null)
                                 .ToList();
            foreach (var workNotification in workNotifications)
            {
                workNotification.DateExpired = deadline.DateExpired ?? deadline.DateComplete;
                workNotification.DescriptionExpired = deadline.DescriptionExpired;
                workNotification.UserExpiredId = deadline.UserExpiredId ?? deadline.UserId;
            }
        }

        /// <summary>
        /// Запис на срок
        /// </summary>
        /// <param name="deadline">Попълнен обект за срок</param>
        /// <returns></returns>
        private bool SaveDeadLine(CaseDeadline deadline)
        {
            if (deadline != null)
            {
                if (deadline.SourceType == SourceTypeSelectVM.CaseSession)
                {
                    if (string.IsNullOrEmpty(workNotificationService.GetJudgeUserId(deadline.CaseId, (int)deadline.SourceId)))
                        return false;
                }
                else
                {
                    if (string.IsNullOrEmpty(workNotificationService.GetJudgeUserId(deadline.CaseId)))
                        return false;
                }

                deadline.UserId = ImpersonatedUserId ?? userContext.UserId;
                deadline.DateWrt = DateTime.Now;
                deadline.CourtId = ((deadline.CourtId ?? 0) <= 0) ? (ImpersonatedCourtId ?? userContext.CourtId) : deadline.CourtId;

                if (deadline.DateComplete == null && deadline.DateExpired == null)
                {
                    List<WorkNotification> workNotifications = workNotificationService.NewWorkNotification(deadline).Result;

                    if (deadline.Id == 0)
                    {
                        if (workNotifications?.Any() == true)
                        {
                            foreach (var workNotification in workNotifications)
                            {
                                workNotification.CaseDeadline = deadline;
                            }
                            repo.AddRange(workNotifications);
                        }
                        else
                            repo.Add(deadline);
                    }
                    else
                    {
                        repo.Update(deadline);

                        if (workNotifications?.Any() == true)
                        {
                            foreach (var workNotification in workNotifications)
                            {
                                workNotification.CaseDeadlineId = deadline.Id;
                                if (workNotification.Id > 0)
                                    repo.Update(workNotification);
                                else
                                    repo.Add(workNotification);
                            }
                        }
                    }

                    return true;
                }

                if (deadline.DateComplete != null || deadline.DateExpired != null)
                {
                    if (deadline.Id == 0)
                        repo.Add(deadline);
                    else
                        repo.Update(deadline);

                    ExpireWorkNotifications(deadline);
                }
            }

            return false;
        }

        private void SaveDeadLineExpired(List<CaseDeadline> deadlines)
        {
            foreach (var deadline in deadlines)
            {
                setExpired(deadline);
                SaveDeadLine(deadline);
            }
        }

        public IQueryable<CaseDeadLineVM> CaseDeadLineSelect(CaseDeadLineFilterVM filter)
        {
            var users = repo.AllReadonly<CaseSessionMeetingUser>();

            int? courtId = (ImpersonatedCourtId ?? userContext.CourtId);

            var deadlines = repo.AllReadonly<CaseDeadline>()
                                .Where(x => x.CourtId == courtId)
                                .Where(x => x.DateExpired == null)
                                .Where(x => x.DateComplete == null);

            if (filter.DateStartFrom != null)
                deadlines = deadlines.Where(x => x.StartDate >= filter.DateStartFrom.Value.Date);

            if (filter.DateStartTo != null)
                deadlines = deadlines.Where(x => x.StartDate.Date <= filter.DateEndTo);

            if (filter.DateEndFrom != null)
                deadlines = deadlines.Where(x => x.EndDate >= filter.DateEndFrom.Value.Date);

            if (filter.DateEndTo != null)
                deadlines = deadlines.Where(x => x.EndDate.Date <= filter.DateEndTo);

            if (filter.CaseId > 0)
                deadlines = deadlines.Where(x => x.CaseId == filter.CaseId);

            if (filter.CaseGroupId > 0)
                deadlines = deadlines.Where(x => x.Case.CaseGroupId == filter.CaseGroupId);

            if (filter.LawUnitId > 0)
                deadlines = deadlines.Where(x => x.Case
                                                  .CaseLawUnits
                                                  .Any(l => l.LawUnitId == filter.LawUnitId &&
                                                            l.CaseSessionId == null &&
                                                            l.DateTo == null &&
                                                            l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter));

            if (!string.IsNullOrEmpty(filter.RegNumber))
                deadlines = deadlines.Where(x => EF.Functions.ILike(x.Case.RegNumber, filter.RegNumber.ToCasePaternSearch()));

            if (filter.DeadlineTypeId > 0)
                deadlines = deadlines.Where(x => x.DeadlineTypeId == filter.DeadlineTypeId);

            return deadlines.Select(x => new CaseDeadLineVM()
            {
                Id = x.Id,
                CaseId = x.CaseId,
                CaseInfo = x.Case.RegNumber + " " + (x.Case.CaseType.Code ?? ""),
                MakerName = (x.SourceType == SourceTypeSelectVM.CaseSession &&
                             x.Case.CaseLawUnits.Where(l => l.CaseSessionId == x.SourceId &&
                                                            l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                .Any() ? string.Join("<br>", x.Case
                                                                              .CaseLawUnits
                                                                              .Where(l => l.CaseSessionId == x.SourceId &&
                                                                                          l.DateTo == null &&
                                                                                          x.SourceType == SourceTypeSelectVM.CaseSession &&
                                                                                          l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                              .Select(n => n.LawUnit != null ? n.LawUnit.FullName : null)) :
                                                        string.Join("<br>", x.Case
                                                                             .CaseLawUnits
                                                                             .Where(l => l.CaseSessionId == null &&
                                                                                         l.DateTo == null &&
                                                                                         l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                                                             .Select(n => n.LawUnit != null ? n.LawUnit.FullName : "")))
                            + "<br>" +
                            (x.DeadlineTypeId != NomenclatureConstants.DeadlineType.OpenSessionResult ? "" : string.Join("<br>", users.Where(u => u.CaseSessionMeeting.CaseSessionId == x.SourceId)
                                                                                                                                      .Select(s => s.SecretaryUser.LawUnit.FullName))),
                DeadlineGroup = x.DeadlineGroup.Label,
                DeadlineType = x.DeadlineType.Label,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                DateComplete = x.DateComplete,
                SourceId = x.SourceId,
                SourceType = x.SourceType,
                SourceUrl = x.SourceType == SourceTypeSelectVM.CaseNotification ? urlHelper.Action("Edit", "CaseNotification", new { id = x.SourceId }) :
                                                                                  (x.SourceType == SourceTypeSelectVM.CaseSession ? urlHelper.Action("Preview", "CaseSession", new { id = x.SourceId, tab = "tabname" }).Replace("tabname", "", StringComparison.InvariantCultureIgnoreCase) : string.Empty)
            });
        }
        public string GetTaskObjectUrl(int sourceType, long sourceId)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.CaseNotification:
                    return urlHelper.Action("Edit", "CaseNotification", new { id = sourceId });
                case SourceTypeSelectVM.CaseSession:
                    return urlHelper.Action("Preview", "CaseSession", new { id = sourceId, tab = "tabname" }).Replace("tabname", "", StringComparison.InvariantCultureIgnoreCase);
                default:
                    return string.Empty;
            }
        }

        #region DeclaredForResolve

        public void DeadLineDeclaredForResolve(CaseSessionResult sessionResult)
        {
            {
                var deadline = DeadLineDeclaredForResolveStart(sessionResult);
                SaveDeadLine(deadline);
            }
            var deadlines = DeadLineDeclaredForResolveExpire(sessionResult);
            foreach (var deadlineExp in deadlines)
            {
                repo.Update(deadlineExp);
                ExpireWorkNotifications(deadlineExp);
            }
        }
        public CaseDeadline DeadLineDeclaredForResolveStart(CaseSessionResult sessionResult)
        {
            CaseSession session = repo.AllReadonly<CaseSession>()
                                      .Include(x => x.CaseSessionActs)
                                      .Where(x => x.Id == sessionResult.CaseSessionId).FirstOrDefault();
            if (session == null)
                return null;
            var deadline = repo.AllReadonly<CaseDeadline>()
                               .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                           x.SourceId == session.Id &&
                                           x.CaseSessionResultId == sessionResult.Id &&
                                           x.DeadlineTypeId == NomenclatureConstants.DeadlineType.DeclaredForResolve)
                               .FirstOrDefault();
            var aCase = session.Case;
            if (aCase == null)
                aCase = repo.AllReadonly<Case>().Where(x => x.Id == session.CaseId).FirstOrDefault();
            var deadlineType = repo.AllReadonly<DeadlineType>().Where(x => x.Id == NomenclatureConstants.DeadlineType.DeclaredForResolve).FirstOrDefault();
            bool isSet = false;
            if (deadline != null)
            {
                if (sessionResult.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution ||
                    sessionResult.DateExpired != null)
                {
                    if (deadline.DateExpired != null)
                    {
                        setUnExpired(deadline);
                        isSet = true;
                    }
                    if (deadline.StartDate != session.DateTo && session.DateTo != null)
                    {
                        deadline.StartDate = session.DateTo ?? deadline.StartDate;
                        setDateEnd(deadline, deadlineType);
                        isSet = true;
                    }
                }
                else
                {
                    setExpired(deadline);
                    isSet = true;
                }
            }
            else
            {
                if (sessionResult.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)
                {
                    if (session.CaseSessionActs.Any(x => x.DateExpired == null && x.ActDeclaredDate != null && x.ActTypeId == NomenclatureConstants.ActType.Answer))
                        return null;
                    deadline = new CaseDeadline();
                    deadline.CaseId = session.CaseId;
                    deadline.SourceType = SourceTypeSelectVM.CaseSession;
                    deadline.SourceId = session.Id;
                    deadline.DeadlineTypeId = NomenclatureConstants.DeadlineType.DeclaredForResolve;
                    deadline.DeadlineGroupId = deadlineType.DeadlineGroupId;
                    deadline.CaseSessionResultId = sessionResult.Id;
                    deadline.StartDate = session.DateTo ?? DateTime.Now;
                    setDateEnd(deadline, deadlineType);
                    isSet = true;
                }
            }
            if (deadline != null && isSet)
            {
                return deadline;
            }
            else
            {
                return null;
            }
        }
        public List<CaseDeadline> DeadLineDeclaredForResolveExpire(CaseSessionResult sessionResult)
        {
            CaseSession session = repo.AllReadonly<CaseSession>().Where(x => x.Id == sessionResult.CaseSessionId).FirstOrDefault();
            var result = new List<CaseDeadline>();
            if (sessionHaveResult(session, NomenclatureConstants.CaseSessionResult.StopedMoveWithSubstantialReason))
            {
                var deadlines = repo.AllReadonly<CaseDeadline>()
                                   .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                               x.SourceId <= session.Id &&
                                               x.CaseId == session.CaseId &&
                                               x.DeadlineTypeId == NomenclatureConstants.DeadlineType.DeclaredForResolve &&
                                               x.DateComplete == null &&
                                               x.DateExpired == null)
                                   .ToList();
                foreach (var deadline in deadlines)
                {
                    setExpired(deadline);
                    deadline.ResultExpiredId = sessionResult.Id;
                    result.Add(deadline);
                }
            }
            if (!sessionHaveResult(session, NomenclatureConstants.CaseSessionResult.AnnouncedForResolution))
            {
                var deadlines = repo.AllReadonly<CaseDeadline>()
                                   .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                               x.SourceId <= session.Id &&
                                               x.CaseId == session.CaseId &&
                                               x.DeadlineTypeId == NomenclatureConstants.DeadlineType.DeclaredForResolve &&
                                               x.DateComplete == null &&
                                               x.DateExpired == null)
                                   .ToList();
                foreach (var deadline in deadlines)
                {
                    setExpired(deadline);
                    deadline.ResultExpiredId = sessionResult.Id;
                    if (!result.Any(x => x.Id == deadline.Id))
                        result.Add(deadline);
                }
            }
            return result;
        }
        public List<CaseDeadline> DeadLineDeclaredForResolveExpireOnSession(CaseSession session)
        {
            var result = new List<CaseDeadline>();
            if (!sessionHaveResult(session, NomenclatureConstants.CaseSessionResult.AnnouncedForResolution) ||
                session.DateExpired != null)
            {
                var deadlines = repo.AllReadonly<CaseDeadline>()
                                   .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                               x.SourceId == session.Id &&
                                               x.CaseId == session.CaseId &&
                                               x.DeadlineTypeId == NomenclatureConstants.DeadlineType.DeclaredForResolve &&
                                               x.DateComplete == null &&
                                               x.DateExpired == null)
                                   .ToList();
                foreach (var deadline in deadlines)
                {
                    setExpired(deadline);
                    result.Add(deadline);
                }
            }
            return result;
        }
        public void DeadLineDeclaredForResolveComplete(Case aCase)
        {
            var result = new List<CaseDeadline>();
            if (aCase.CaseStateId == NomenclatureConstants.CaseState.Resolution)
            {
                var deadlines = repo.AllReadonly<CaseDeadline>()
                                   .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                               x.CaseId == aCase.Id &&
                                               x.DeadlineTypeId == NomenclatureConstants.DeadlineType.DeclaredForResolve &&
                                               x.DateComplete == null &&
                                               x.DateExpired == null)
                                   .ToList();
                foreach (var deadline in deadlines)
                {
                    deadline.DateComplete = DateTime.Now;
                    result.Add(deadline);
                }
            }
            foreach (var deadlineComplete in result)
            {
                repo.Update(deadlineComplete);
                ExpireWorkNotifications(deadlineComplete);
            }
        }


        public void DeadLineDeclaredForResolveComplete(CaseSessionAct caseSessionAct)
        {
            var result = new List<CaseDeadline>();
            if (caseSessionAct.ActTypeId == NomenclatureConstants.ActType.Answer)
            {
                var deadlines = repo.AllReadonly<CaseDeadline>()
                                   .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                               x.CaseId == caseSessionAct.CaseId &&
                                               x.DeadlineTypeId == NomenclatureConstants.DeadlineType.DeclaredForResolve &&
                                               x.DateComplete == null &&
                                               x.DateExpired == null)
                                   .ToList();
                foreach (var deadline in deadlines)
                {
                    deadline.DateComplete = DateTime.Now;
                    result.Add(deadline);
                }
            }
            foreach (var deadlineComplete in result)
            {
                repo.Update(deadlineComplete);
                ExpireWorkNotifications(deadlineComplete);
            }
        }

        #endregion DeclaredForResolve

        #region Motive

        private DateTime? MotiveDateStart(CaseSessionAct sessionAct)
        {
            var dateStart = sessionAct.ActDate?.AddDays(1).Date;// ActDeclaredDate
            if (dateStart != null)
                while (!workingDaysService.IsWorkingDay(ImpersonatedCourtId ?? userContext.CourtId, dateStart.Value))
                {
                    dateStart = dateStart.Value.AddDays(1).Date;
                }
            return dateStart;
        }
        private DateTime? MotiveDateEnd(CaseSessionAct sessionAct)
        {
            return sessionAct.ActMotivesDeclaredDate;
        }
        public void DeadLineMotive(CaseSessionAct sessionAct)
        {
            if (sessionAct.ActTypeId != NomenclatureConstants.ActType.Sentence)
                return;

            var deadline = DeadLineMotiveStart(sessionAct);
            if (deadline != null)
            {
                SaveDeadLine(deadline);
                return;
            }
            deadline = DeadLineMotiveExpire(sessionAct);
            if (deadline != null)
            {
                SaveDeadLine(deadline);
                return;
            }
            deadline = DeadLineMotiveComplete(sessionAct);
            if (deadline != null)
            {
                SaveDeadLine(deadline);
                return;
            }
        }

        public CaseDeadline DeadLineMotiveStart(CaseSessionAct sessionAct)
        {
            if (sessionAct.DateExpired != null || MotiveDateStart(sessionAct) == null || MotiveDateEnd(sessionAct) != null)
                return null;
            var deadline = repo.AllReadonly<CaseDeadline>()
                      .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct &&
                                  x.SourceId == sessionAct.Id &&
                                  x.DeadlineTypeId == NomenclatureConstants.DeadlineType.Motive)
                      .FirstOrDefault();

            var aCase = sessionAct.CaseSession?.Case;
            if (aCase == null)
            {
                var caseSession = sessionAct.CaseSession;
                if (caseSession == null)
                    caseSession = repo.AllReadonly<CaseSession>().Where(x => x.Id == sessionAct.CaseSessionId).FirstOrDefault();
                aCase = repo.AllReadonly<Case>().Where(x => x.Id == caseSession.CaseId).FirstOrDefault();
            }
            var deadlineType = repo.AllReadonly<DeadlineType>().Where(x => x.Id == NomenclatureConstants.DeadlineType.Motive).FirstOrDefault();

            if (deadline != null)
            {
                if (deadline.StartDate != MotiveDateStart(sessionAct))
                {
                    setUnExpired(deadline);
                    deadline.StartDate = MotiveDateStart(sessionAct) ?? DateTime.Now;
                    setDateEnd(deadline, deadlineType);
                }
            }
            else
            {
                if (repo.AllReadonly<CaseSessionAct>()
                        .Where(x => x.CaseId == sessionAct.CaseId &&
                               x.ActTypeId == NomenclatureConstants.ActType.Answer &&
                               x.ActDeclaredDate != null &&
                               x.DateExpired != null)
                        .Any()
                   )
                    return null;

                deadline = new CaseDeadline();
                deadline.CaseId = aCase.Id;
                deadline.SourceType = SourceTypeSelectVM.CaseSessionAct;
                deadline.SourceId = sessionAct.Id;
                deadline.DeadlineTypeId = NomenclatureConstants.DeadlineType.Motive;
                deadline.DeadlineGroupId = deadlineType.DeadlineGroupId;
                deadline.StartDate = MotiveDateStart(sessionAct) ?? DateTime.Now;
                setDateEnd(deadline, deadlineType);
            }
            return deadline;
        }
        public CaseDeadline DeadLineMotiveExpire(CaseSessionAct sessionAct)
        {
            if (sessionAct.DateExpired == null)
                return null;
            var deadline = repo.AllReadonly<CaseDeadline>()
                      .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct &&
                                  x.SourceId == sessionAct.Id &&
                                  x.DeadlineTypeId == NomenclatureConstants.DeadlineType.Motive)
                      .FirstOrDefault();
            if (deadline != null && deadline.DateExpired == null)
            {
                setExpired(deadline);
                return deadline;
            }
            return null;
        }

        public CaseDeadline DeadLineMotiveComplete(CaseSessionAct sessionAct)
        {
            if (MotiveDateStart(sessionAct) == null || MotiveDateEnd(sessionAct) == null)
                return null;
            var deadline = repo.AllReadonly<CaseDeadline>()
                      .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct &&
                                  x.SourceId == sessionAct.Id &&
                                  x.DeadlineTypeId == NomenclatureConstants.DeadlineType.Motive)
                      .FirstOrDefault();
            if (deadline != null && deadline.DateComplete == null)
            {
                deadline.DateComplete = MotiveDateEnd(sessionAct);
                return deadline;
            }
            return null;
        }

        #endregion Motive

        #region OpenSessionResult

        public void DeadLineOpenSessionResult(CaseSession session)
        {
            var deadline = DeadLineOpenSessionResultStart(session);
            if (deadline != null)
            {
                SaveDeadLine(deadline);
            }
            else
            {
                var deadlines = repo.AllReadonly<CaseDeadline>()
                     .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                 x.SourceId == session.Id &&
                                 x.DeadlineTypeId == NomenclatureConstants.DeadlineType.OpenSessionResult &&
                                 x.DateExpired == null)
                     .ToList();
                SaveDeadLineExpired(deadlines);
            }
            // Да се направи work_notification na секретарите
            var oldDeadLine = repo.AllReadonly<CaseDeadline>()
                     .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                 x.SourceId == session.Id &&
                                 x.DeadlineTypeId == NomenclatureConstants.DeadlineType.OpenSessionResult &&
                                 x.DateExpired == null)
                     .FirstOrDefault();
            if (oldDeadLine != null && oldDeadLine.Id > 0)
            {
                var workNotifications = workNotificationService.NewWorkNotificationSecretary(oldDeadLine);
                foreach (var notification in workNotifications)
                {
                    notification.CaseDeadlineId = oldDeadLine.Id;
                    if (notification.Id == 0)
                        repo.Add(notification);
                    else
                        repo.Update(notification);
                }
            }
        }
        public void DeadLineOpenSessionResult(CaseSessionMeetingUser user)
        {
            var meeting = repo.AllReadonly<CaseSessionMeeting>()
                              .Where(x => x.Id == user.CaseSessionMeetingId)
                              .FirstOrDefault();
            if (meeting == null)
                return;
            var session = repo.AllReadonly<CaseSession>()
                              .Where(x => x.Id == meeting.CaseSessionId)
                              .FirstOrDefault();
            if (session == null)
                return;
            var oldDeadLine = repo.AllReadonly<CaseDeadline>()
                                 .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                             x.SourceId == session.Id &&
                                             x.DeadlineTypeId == NomenclatureConstants.DeadlineType.OpenSessionResult)
                                 .FirstOrDefault();
            if (oldDeadLine != null && oldDeadLine.Id > 0)
            {
                var workNotifications = workNotificationService.NewWorkNotificationSecretary(oldDeadLine);
                foreach (var notification in workNotifications)
                {
                    notification.CaseDeadlineId = oldDeadLine.Id;
                    if (notification.Id == 0)
                        repo.Add(notification);
                    else
                        repo.Update(notification);
                }
            }

        }
        public CaseDeadline DeadLineOpenSessionResultStart(CaseSession session)
        {
            var aSessionType = session.SessionType;
            if (aSessionType == null)
                aSessionType = repo.AllReadonly<SessionType>()
                                   .Where(x => x.Id == session.SessionTypeId)
                                   .FirstOrDefault();
            if (aSessionType == null)
                return null;
            if (aSessionType.SessionTypeGroup == NomenclatureConstants.CaseSessionTypeGroup.PrivateSession)
                return null;
            if (session.SessionStateId != NomenclatureConstants.SessionState.Provedeno)
                return null;
            var deadline = repo.AllReadonly<CaseDeadline>()
                      .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                  x.SourceId == session.Id &&
                                  x.DeadlineTypeId == NomenclatureConstants.DeadlineType.OpenSessionResult)
                      .FirstOrDefault();
            if (session.DateExpired != null)
            {
                if (deadline != null && deadline.DateExpired == null)
                {
                    setExpired(deadline);
                    return deadline;
                }
                return null;
            }
            var aCase = session.Case;
            if (aCase == null)
                aCase = repo.AllReadonly<Case>().Where(x => x.Id == session.CaseId).FirstOrDefault();

            var deadlineType = repo.AllReadonly<DeadlineType>().Where(x => x.Id == NomenclatureConstants.DeadlineType.OpenSessionResult).FirstOrDefault();

            var sessionMeeting = repo.AllReadonly<CaseSessionMeeting>()
                                    .Where(x => x.CaseSessionId == session.Id)
                                    .OrderBy(x => x.DateTo)
                                    .LastOrDefault();
            var startDate = (sessionMeeting?.DateTo ?? session.DateTo) ?? DateTime.Now;
            startDate = startDate.AddDays(1);
            if (deadline != null)
            {
                if (deadline.StartDate.Date != startDate.Date || deadline.DateExpired != null)
                {
                    setUnExpired(deadline);
                    deadline.StartDate = startDate;
                    setDateEnd(deadline, deadlineType);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (repo.AllReadonly<CaseSessionAct>()
                        .Where(x => x.CaseId == session.CaseId &&
                                    x.CaseSessionId == session.Id &&
                                    (x.ActTypeId == NomenclatureConstants.ActType.Protokol ||
                                     x.ActTypeId == NomenclatureConstants.ActType.ProtokolOpredelenie ||
                                     x.ActTypeId == NomenclatureConstants.ActType.Agreement) &&
                                    x.ActDeclaredDate != null &&
                                    x.DateExpired == null)
                        .Any()
                   )
                    return null;
                deadline = new CaseDeadline();
                deadline.CaseId = aCase.Id;
                deadline.SourceType = SourceTypeSelectVM.CaseSession;
                deadline.SourceId = session.Id;
                deadline.DeadlineTypeId = NomenclatureConstants.DeadlineType.OpenSessionResult;
                deadline.DeadlineGroupId = deadlineType.DeadlineGroupId;
                deadline.StartDate = startDate;
                setDateEnd(deadline, deadlineType);
            }
            return deadline;
        }
        public void DeadLineOpenSessionResultComplete(CaseSessionResult sessionResult)
        {
            var deadline = DeadLineOpenSessionResultCompleteInit(sessionResult);
            SaveDeadLine(deadline);
        }
        public CaseDeadline DeadLineOpenSessionResultCompleteInit(CaseSessionResult sessionResult)
        {
            CaseSession session = repo.AllReadonly<CaseSession>().Where(x => x.Id == sessionResult.CaseSessionId).FirstOrDefault();
            var deadline = repo.AllReadonly<CaseDeadline>()
                      .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                  x.SourceId == session.Id &&
                                  x.DeadlineTypeId == NomenclatureConstants.DeadlineType.OpenSessionResult &&
                                  x.DateComplete == null)
                      .FirstOrDefault();
            if (deadline == null)
                return null;
            deadline.DateComplete = DateTime.Now;
            deadline.CaseSessionResultId = sessionResult.Id;
            return deadline;
        }

        public void DeadLineOpenSessionResultCompleteOnAct(CaseSessionAct caseSessionAct)
        {
            var result = new List<CaseDeadline>();
            if (caseSessionAct.ActTypeId == NomenclatureConstants.ActType.Protokol ||
                caseSessionAct.ActTypeId == NomenclatureConstants.ActType.ProtokolOpredelenie ||
                caseSessionAct.ActTypeId == NomenclatureConstants.ActType.Agreement)
            {
                var deadlines = repo.AllReadonly<CaseDeadline>()
                                   .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession &&
                                               x.CaseId == caseSessionAct.CaseId &&
                                               x.DeadlineTypeId == NomenclatureConstants.DeadlineType.OpenSessionResult &&
                                               x.DateComplete == null &&
                                               x.DateExpired == null)
                                   .ToList();
                foreach (var deadline in deadlines)
                {
                    deadline.DateComplete = DateTime.Now;
                    result.Add(deadline);
                }
            }
            foreach (var deadlineComplete in result)
            {
                repo.Update(deadlineComplete);
                ExpireWorkNotifications(deadlineComplete);
            }
        }

        #endregion OpenSessionResult

        #region CompanyCase

        public void DeadLineCompanyCase(Case companyCase)
        {
            var deadlines = DeadLineCompanyCaseStart(companyCase);
            foreach (var deadline in deadlines)
            {
                SaveDeadLine(deadline);
            }

            var deadlinesComplete = DeadLineCompanyCaseComplete(companyCase);
            foreach (var deadline in deadlinesComplete)
            {
                SaveDeadLine(deadline);
            }
        }
        public void DeadLineCompanyCaseByCaseId(int caseId)
        {
            var companyCase = repo.AllReadonly<Case>()
                           .Where(x => x.Id == caseId)
                           .FirstOrDefault();
            if (companyCase != null)
            {
                DeadLineCompanyCase(companyCase);
                repo.SaveChanges();
            }
        }
        public void DeadLineCompanyCaseStartOnDocument(Infrastructure.Data.Models.Documents.Document document)
        {
            var companyCase = repo.AllReadonly<DocumentCaseInfo>()
                            .Where(x => x.DocumentId == document.Id)
                            .Select(x => x.Case)
                            .FirstOrDefault();
            if (companyCase == null)
                return;
            var deadlines = DeadLineCompanyCaseStart(companyCase);
            foreach (var deadline in deadlines)
            {
                SaveDeadLine(deadline);
            }
        }
        public void DeadLineCompanyCaseCompleteOnResult(CaseSessionResult sessionResult)
        {
            var companyCase = repo.AllReadonly<CaseSession>()
                            .Where(x => x.Id == sessionResult.CaseSessionId)
                            .Select(x => x.Case)
                            .FirstOrDefault();
            if (companyCase == null)
                return;
            var deadlinesComplete = DeadLineCompanyCaseComplete(companyCase);
            foreach (var deadline in deadlinesComplete)
            {
                SaveDeadLine(deadline);
            }
        }

        public List<CaseDeadline> DeadLineCompanyCaseStart(Case companyCase)
        {
            var result = new List<CaseDeadline>();
            if (!nomService.CaseCodeGroup_Check(NomenclatureConstants.CaseCodeGroupAlias.CaseCompanyRegister, companyCase.CaseCodeId ?? 0))
                return result;
            var documents = repo.AllReadonly<DocumentCaseInfo>()
                                .Where(x => x.CaseId == companyCase.Id)
                                .Where(x => x.Document.DocumentTypeId == NomenclatureConstants.DocumentType.ApplicationForCompanyRegister ||
                                            x.Document.DocumentTypeId == NomenclatureConstants.DocumentType.ApplicationForCompanyChange)
                                .Select(x => x.Document)
                                .ToList();
            if (!documents.Any(x => x.Id == companyCase.DocumentId))
            {
                var document = repo.AllReadonly<Infrastructure.Data.Models.Documents.Document>()
                                .Where(x => x.Id == companyCase.DocumentId)
                                .Where(x => x.DocumentTypeId == NomenclatureConstants.DocumentType.ApplicationForCompanyRegister ||
                                            x.DocumentTypeId == NomenclatureConstants.DocumentType.ApplicationForCompanyChange)
                                .FirstOrDefault();
                if (document != null)
                    documents.Add(document);
            }
            foreach (var aDocument in documents)
            {
                int deadlineTypeId = 0;
                if (aDocument.DocumentTypeId == NomenclatureConstants.DocumentType.ApplicationForCompanyRegister)
                    deadlineTypeId = NomenclatureConstants.DeadlineType.CompanyCaseRegister;
                if (aDocument.DocumentTypeId == NomenclatureConstants.DocumentType.ApplicationForCompanyChange)
                    deadlineTypeId = NomenclatureConstants.DeadlineType.CompanyCaseChange;
                if (deadlineTypeId == 0)
                    continue;

                var deadline = repo.AllReadonly<CaseDeadline>()
                                   .Where(x => x.SourceType == SourceTypeSelectVM.Document &&
                                               x.SourceId == aDocument.Id &&
                                               (x.DeadlineTypeId == deadlineTypeId))
                                   .FirstOrDefault();

                var deadlineType = repo.AllReadonly<DeadlineType>().Where(x => x.Id == deadlineTypeId).FirstOrDefault();

                bool isSet = false;
                if (deadline != null)
                {
                    if (deadline.StartDate != aDocument.DocumentDate.Date)
                    {
                        deadline.StartDate = aDocument.DocumentDate.Date;
                        setDateEnd(deadline, deadlineType);
                        isSet = true;
                    }
                }
                else
                {
                    deadline = new CaseDeadline();
                    deadline.CaseId = companyCase.Id;
                    deadline.SourceType = SourceTypeSelectVM.Document;
                    deadline.SourceId = aDocument.Id;
                    deadline.DeadlineTypeId = deadlineTypeId;
                    deadline.DeadlineGroupId = deadlineType.DeadlineGroupId;
                    deadline.StartDate = aDocument.DocumentDate.Date;
                    setDateEnd(deadline, deadlineType);
                    isSet = true;
                }
                if (deadline != null && isSet)
                    result.Add(deadline);
            }
            return result;
        }
        public List<CaseDeadline> DeadLineCompanyCaseComplete(Case companyCase)
        {
            var result = new List<CaseDeadline>();
            var sessions = repo.AllReadonly<CaseSession>()
                               .Where(x => x.CaseId == companyCase.Id);
            var deadlines = repo.AllReadonly<CaseDeadline>()
                      .Where(x => x.SourceType == SourceTypeSelectVM.Document &&
                                  // x.SourceId == companyCase.Id &&
                                  x.CaseId == companyCase.Id &&
                                  x.DateComplete == null &&
                                  (x.DeadlineTypeId == NomenclatureConstants.DeadlineType.CompanyCaseRegister ||
                                   x.DeadlineTypeId == NomenclatureConstants.DeadlineType.CompanyCaseChange))
                      .ToList();
            foreach (var deadline in deadlines)
            {
                var sessionResult = repo.AllReadonly<CaseSessionResult>()
                                         .Where(x => x.IsActive)
                                         .Where(x => x.CaseSession.CaseId == companyCase.Id &&
                                                     x.CaseSession.DateFrom >= deadline.StartDate.Date)
                                         .OrderBy(x => x.Id)
                                         .Select(x => new
                                         {
                                             x.Id,
                                             DateFrom = (x.CaseSessionId > 0) ? x.CaseSession.DateFrom : DateTime.Now
                                         })
                                         .FirstOrDefault();

                if (sessionResult != null)
                {
                    deadline.DateComplete = sessionResult.DateFrom;
                    deadline.CaseSessionResultId = sessionResult.Id;
                    result.Add(deadline);
                }
            }
            return result;
        }

        #endregion CompanyCase

        #region Заповедно произвдство

        /// <summary>
        /// Метод връщащ обект тип на срок
        /// </summary>
        /// <param name="deadlineTypeId">Идентификатор на записа</param>
        /// <returns></returns>
        private async Task<DeadlineType> GetDeadlineType(int deadlineTypeId)
        {
            return await repo.AllReadonly<DeadlineType>()
                             .Where(x => x.Id == deadlineTypeId)
                             .FirstAsync();
        }

        /// <summary>
        /// Проверка дали съществува срок за това дело
        /// </summary>
        /// <param name="sourceId">SourceId</param>
        /// <param name="sourceType">SourceType</param>
        /// <param name="dedlineTypeId">Идентификатор на тип на срок</param>
        /// <returns></returns>
        private async Task<bool> IsExistsDeadlineFastProcess(long sourceId, int sourceType, int dedlineTypeId)
        {
            return await repo.AllReadonly<CaseDeadline>()
                             .AnyAsync(d => d.DeadlineTypeId == dedlineTypeId &&
                                            d.SourceId == sourceId &&
                                            d.SourceType == sourceType);
        }

        /// <summary>
        /// Приключване на срок за дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="sourceId">SourceId</param>
        /// <param name="sourceType">SourceType</param>
        /// <param name="dedlineTypeId">Идентификатор на тип на срок</param>
        /// <param name="isComplete">Флаг който показва дали е приключване или сторно на срока</param>
        /// <returns></returns>
        public async Task<bool> CompleteExpiredCaseDeadlineFastProcess(int caseId, long? sourceId, int sourceType, int dedlineTypeId, bool isComplete = true)
        {
            Expression<Func<CaseDeadline, bool>> sourceIdSearch = x => true;
            if (sourceId != null)
                sourceIdSearch = d => d.SourceId == sourceId;

            Expression<Func<CaseDeadline, bool>> completeExpiredSearch = x => x.DateComplete == null;
            if (!isComplete)
                completeExpiredSearch = x => x.DateExpired == null;

            List<CaseDeadline> deadlines = await repo.All<CaseDeadline>()
                                                     .Where(d => d.CaseId == caseId &&
                                                                 d.SourceType == sourceType &&
                                                                 d.DeadlineTypeId == dedlineTypeId)
                                                     .Where(sourceIdSearch)
                                                     .Where(completeExpiredSearch)
                                                     .ToListAsync();

            if (deadlines == null || deadlines.Count() < 1) return false;

            foreach (CaseDeadline deadline in deadlines)
            {
                if (isComplete)
                {
                    deadline.DateComplete = DateTime.Now;

                    List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                     .Where(n => n.CaseDeadlineId == deadline.Id)
                                                                     .ToListAsync();

                    if (notifications != null && notifications.Count() > 0)
                        notifications.ForEach(n => { n.DateTurnOff = DateTime.Now; });
                }
                else
                {
                    deadline.DateExpired = DateTime.Now;
                    deadline.UserExpiredId = ImpersonatedUserId ?? userContext.UserId;
                    SaveDeadLine(deadline);
                }
            }

            return true;
        }

        /// <summary>
        /// Валидация при създаване на срок за бързо производство
        /// </summary>
        /// <param name="documentRequestTypeCode">Тип на документа от което се взема дали е бързо производство</param>
        /// <param name="sourceId">SourceId</param>
        /// <param name="sourceType">SourceType</param>
        /// <param name="dedlineTypeId">Идентификатор на тип на срок</param>
        /// <returns></returns>
        private async Task<bool> ValidateForStartDeadlineFastProcess(string documentRequestTypeCode, long sourceId, int sourceType, int dedlineTypeId)
        {
            if (!DocumentConstants.ElectronicDocumentRequestTypes.FastProcess.Contains(documentRequestTypeCode))
                return false;

            if (await IsExistsDeadlineFastProcess(sourceId, sourceType, dedlineTypeId))
                return false;

            return true;
        }

        /// <summary>
        /// Стартиране на срок за предприемане на действия по новообразувано дело N2
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<bool> StartTakingActionFastProcess(int caseId)
        {
            try
            {
                var caseData = await repo.AllReadonly<Case>()
                                     .Where(c => c.Id == caseId)
                                     .Select(c => new
                                     {
                                         CaseId = c.Id,
                                         c.CourtId,
                                         DocumentRequestTypeCode = (c.Document.DocumentRequestTypeId != null ? c.Document.DocumentRequestType.RequestCode : string.Empty),
                                         c.RegDate
                                     })
                                     .FirstOrDefaultAsync();

                if (!await ValidateForStartDeadlineFastProcess(caseData.DocumentRequestTypeCode, caseId, SourceTypeSelectVM.Case, NomenclatureConstants.DeadlineType.TakingActionFastProcess))
                    return false;

                Infrastructure.Data.Models.Nomenclatures.DeadlineType deadlineType = await GetDeadlineType(NomenclatureConstants.DeadlineType.TakingActionFastProcess);

                CaseDeadline deadline = new()
                {
                    CaseId = caseData.CaseId,
                    CourtId = caseData.CourtId,
                    SourceType = SourceTypeSelectVM.Case,
                    SourceId = caseData.CaseId,
                    DeadlineTypeId = NomenclatureConstants.DeadlineType.TakingActionFastProcess,
                    DeadlineGroupId = deadlineType.DeadlineGroupId,
                    StartDate = caseData.RegDate,
                    NotificationKind = 2
                };

                setDateEnd(deadline, deadlineType);
                SaveDeadLine(deadline);
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при стартиране на срок за предприемане на действия по новообразувано дело {caseId}");
                return false;
            }

        }

        /// <summary>
        /// Стартиране на срок за липса на произнасяне по съпровождащ документ (или по частна жалба) N5
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        public async Task<bool> StartMissingActForCompliantDocumentFastProcess(long documentId)
        {
            try
            {
                var caseData = await repo.AllReadonly<DocumentCaseInfo>()
                                     .Where(d => d.DocumentId == documentId &&
                                                 d.CaseId != null &&
                                                 d.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                                     .Select(d => new
                                     {
                                         d.CaseId,
                                         CaseCourtId = d.Case.CourtId,
                                         CaseTypeLabel = d.Case.CaseType.Label,
                                         CaseRegNumber = d.Case.RegNumber,
                                         CaseYear = d.Case.RegDate.Year,
                                         CaseDocumentRequestTypeCode = (d.Case.Document.DocumentRequestTypeId != null ? d.Case.Document.DocumentRequestType.RequestCode : string.Empty),
                                         d.Document.DocumentDate,
                                     })
                                     .FirstOrDefaultAsync();

                if (caseData == null)
                    return false;

                if (!await ValidateForStartDeadlineFastProcess(caseData.CaseDocumentRequestTypeCode, documentId, SourceTypeSelectVM.Document, NomenclatureConstants.DeadlineType.MissingActForCompliantDocumentFastProcess))
                    return false;

                DeadlineType deadlineType = await GetDeadlineType(NomenclatureConstants.DeadlineType.MissingActForCompliantDocumentFastProcess);

                CaseDeadline deadline = new()
                {
                    CaseId = caseData.CaseId ?? 0,
                    CourtId = caseData.CaseCourtId,
                    SourceType = SourceTypeSelectVM.Document,
                    SourceId = documentId,
                    DeadlineTypeId = NomenclatureConstants.DeadlineType.MissingActForCompliantDocumentFastProcess,
                    DeadlineGroupId = deadlineType.DeadlineGroupId,
                    StartDate = caseData.DocumentDate,
                    NotificationKind = 2
                };

                setDateEnd(deadline, deadlineType);
                SaveDeadLine(deadline);
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при стартиране на срок за липса на произнасяне по съпровождащ документ (или по частна жалба) {documentId}");
                return false;
            }

        }

        /// <summary>
        /// Приключване / сторно на срок за липса на произнасяне по съпровождащ документ (или по частна жалба) N5
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <param name="isSaveChanges">Флаг дали да има SaveChanges</param>
        /// <param name="isComplete">Флаг за тип операция приключване/сторно</param>
        /// <returns></returns>
        public async Task<bool> CompleteExpiredMissingActForCompliantDocumentFastProcess(int caseSessionActId, bool isSaveChanges, bool isComplete = true)
        {
            try
            {
                var docIds = await repo.AllReadonly<CaseSessionDoc>()
                                       .Where(d => d.CaseSession.CaseSessionActs.Any(a => a.Id == caseSessionActId))
                                       .Select(d => new { d.CaseId, d.DocumentId })
                                       .ToListAsync();

                if (docIds == null || docIds.Count < 1)
                    return false;

                foreach (var docId in docIds)
                {
                    await CompleteExpiredCaseDeadlineFastProcess(docId.CaseId ?? 0, docId.DocumentId, SourceTypeSelectVM.Document, NomenclatureConstants.DeadlineType.MissingActForCompliantDocumentFastProcess, isComplete);
                }

                if (isSaveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                string typeOperation = isComplete ? "приключване" : "анулиране";
                logger.LogError(ex, $"Грешка при {typeOperation} на нотификация за липса на произнасяне по съпровождащ документ (или по частна жалба) N5 CaseSessionActId:{caseSessionActId}");
                return false;
            }
        }

        /// <summary>
        /// Стартиране на срок за невърнато съобщение по чл. 410 ГПК или чл. 417 ГПК N9
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        public async Task<bool> StartUnreturnedMessageFastProcess(int caseNotificationId)
        {
            try
            {
                var caseData = await repo.AllReadonly<CaseNotification>()
                                         .Where(n => n.Id == caseNotificationId)
                                         .Where(n => n.DateExpired == null)
                                         .Where(n => n.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.ByEPEP)
                                         .Where(n => !NomenclatureConstants.HtmlTemplateConstants.NT_47s.Contains(n.HtmlTemplateId ?? 0))
                                         .Select(n => new
                                         {
                                             n.CaseId,
                                             n.CaseSessionId,
                                             CaseCourtId = n.Case.CourtId,
                                             CaseTypeLabel = n.Case.CaseType.Label,
                                             CaseRegNumber = n.Case.RegNumber,
                                             CaseYear = n.Case.RegDate.Year,
                                             CaseDocumentRequestTypeCode = (n.Case.Document.DocumentRequestTypeId != null ? n.Case.Document.DocumentRequestType.RequestCode : string.Empty),
                                             NotificationRegDate = n.RegDate,
                                         })
                                         .FirstOrDefaultAsync();

                if (caseData == null)
                    return false;

                if (!await ValidateForStartDeadlineFastProcess(caseData.CaseDocumentRequestTypeCode, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.DeadlineType.UnreturnedMessageFastProcess))
                    return false;

                DeadlineType deadlineType = await GetDeadlineType(NomenclatureConstants.DeadlineType.UnreturnedMessageFastProcess);

                CaseDeadline deadline = new()
                {
                    CaseId = caseData.CaseId,
                    CourtId = caseData.CaseCourtId,
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    DeadlineTypeId = NomenclatureConstants.DeadlineType.UnreturnedMessageFastProcess,
                    DeadlineGroupId = deadlineType.DeadlineGroupId,
                    StartDate = caseData.NotificationRegDate,
                    NotificationKind = 2
                };

                setDateEnd(deadline, deadlineType);
                SaveDeadLine(deadline);
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при стартиране на срок за  невърнато съобщение по чл. 410 ГПК или чл. 417 ГПК {caseNotificationId}");
                return false;
            }
        }

        /// <summary>
        /// Приключване / сторно на срок за невърнато съобщение по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="isSaveChanges">Флаг дали да има SaveChanges</param>
        /// <param name="isComplete">Флаг за тип операция приключване/сторно</param>
        /// <returns></returns>
        public async Task<bool> CompleteExpiredUnreturnedMessageFastProcess(int caseNotificationId, bool isSaveChanges, bool isComplete = true)
        {
            try
            {
                int caseId = await repo.AllReadonly<CaseNotification>()
                                       .Where(n => n.Id == caseNotificationId)
                                       .Select(n => n.CaseId)
                                       .FirstAsync();

                await CompleteExpiredCaseDeadlineFastProcess(caseId, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.DeadlineType.UnreturnedMessageFastProcess, isComplete);
                if (isSaveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                string typeOperation = isComplete ? "приключване" : "анулиране";
                logger.LogError(ex, $"Грешка при {typeOperation} на нотификация за невърнато съобщение по дело по чл. 410 ГПК или чл. 417 ГПК на CaseNotificationId:{caseNotificationId}");
                return false;
            }
        }

        #endregion
    }
}
