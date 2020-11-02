// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseDeadlineService : BaseService, ICaseDeadlineService
    {
        private readonly IUrlHelper urlHelper;
        private readonly IWorkNotificationService workNotificationService;
        private readonly IWorkingDaysService workingDaysService;
        private readonly INomenclatureService nomService;
        public CaseDeadlineService(
            ILogger<CaseDeadline> _logger,
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
        private bool sessionHaveResult(CaseSession session, int resultId)
        {
            var caseSessionResults = session.CaseSessionResults;
            if (caseSessionResults == null || !caseSessionResults.Any())
                caseSessionResults = repo.AllReadonly<CaseSessionResult>()
                                         .Where(x => x.IsActive)
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
            deadline.UserExpiredId = userContext.UserId;
            deadline.DescriptionExpired = "";
        }
        private void setDateEnd(CaseDeadline deadline, DeadlineType deadlineType, bool isSpecial = false) 
        {
            int? months = isSpecial ? deadlineType.DeadlineSpecialMonths : deadlineType.DeadlineMonths;
            if (months != null)
            {
                int month = months ?? 0;
                deadline.EndDate = deadline.StartDate.AddMonths(month).Date;
                while (!workingDaysService.IsWorkingDay(userContext.CourtId, deadline.EndDate))
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
                    if (workingDaysService.IsWorkingDay(userContext.CourtId, deadline.EndDate))
                        wDays--;
                }

            }

            int? normalDays = isSpecial ? deadlineType.DeadlineSpecialDays : deadlineType.DeadlineDays;
            if (normalDays != null)
            {
                int days = normalDays ?? 0;
                deadline.EndDate = deadline.StartDate.AddDays(days - 1).Date;
                while (!workingDaysService.IsWorkingDay(userContext.CourtId, deadline.EndDate))
                {
                    deadline.EndDate = deadline.EndDate.AddDays(1).Date;
                }
            }
        }
        private bool SaveDeadLine(CaseDeadline deadline)
        {
            if (deadline != null)
            {
                if (workNotificationService.GetJudgeUserId(deadline.CaseId) == null)
                    return false;
                deadline.UserId = userContext.UserId;
                deadline.DateWrt = DateTime.Now;
                if ((deadline.CourtId ?? 0) <= 0)
                    deadline.CourtId = userContext.CourtId;

                if (deadline.DateComplete == null && deadline.DateExpired == null)
                {
                    var workNotification = workNotificationService.NewWorkNotification(deadline);
                    if (workNotification != null)
                    {
                        workNotification.CaseDeadline = deadline;
                        if (workNotification.Id == 0)
                            repo.Add(workNotification);
                        else
                            repo.Update(workNotification);
                    }
                    else
                    {
                        if (deadline.Id == 0)
                            repo.Add(deadline);
                        else
                            repo.Update(deadline);
                    }
                    return true;
                }
                if (deadline.DateComplete != null || deadline.DateExpired != null)
                {
                    if (deadline.Id == 0)
                        repo.Add(deadline);
                    else
                        repo.Update(deadline);
                    var workNotifications = repo.AllReadonly<WorkNotification>()
                                                .Where(x => x.CaseDeadlineId == deadline.Id && x.DateExpired == null)
                                                .ToList();
                    foreach(var workNotification in workNotifications)
                    {
                        workNotification.DateExpired = deadline.DateExpired ?? deadline.DateComplete;
                        workNotification.DescriptionExpired = deadline.DescriptionExpired;
                        workNotification.UserExpiredId = deadline.UserExpiredId ?? deadline.UserId;
                        repo.Update(workNotification);
                    }
                }
            }
            return false;
        }
        #region DeclaredForResolve
        public void DeadLineDeclaredForResolve(CaseSessionResult sessionResult)
        {
            var deadline = DeadLineDeclaredForResolveStart(sessionResult);
            SaveDeadLine(deadline);
            var deadlines = DeadLineDeclaredForResolveExpire(sessionResult);
            foreach (var deadlineExp in deadlines)
                repo.Update(deadlineExp);
        }
        public CaseDeadline DeadLineDeclaredForResolveStart(CaseSessionResult sessionResult)
        {
            CaseSession session = repo.AllReadonly<CaseSession>().Where(x => x.Id == sessionResult.CaseSessionId).FirstOrDefault();
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
                if (sessionResult.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)
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
                } else
                {
                    setExpired(deadline);
                }
            } else {
                if (sessionResult.SessionResultId == NomenclatureConstants.CaseSessionResult.AnnouncedForResolution)
                {
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
            } else
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
                foreach (var deadline in deadlines) {
                    setExpired(deadline);
                    deadline.ResultExpiredId = sessionResult.Id;
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
                repo.Update(deadlineComplete);
        }
        #endregion DeclaredForResolve

        #region Motive
        private DateTime? MotiveDateStart(CaseSessionAct sessionAct)
        {
            return sessionAct.ActDate;// ActDeclaredDate
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
            SaveDeadLine(deadline);
            // Да се направи work_notification na секретарите
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
            if (session.DateExpired != null) {
                if (deadline != null && deadline.DateExpired == null) {
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
                } else
                {
                    return null;
                }
            }
            else
            {
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
        public void DeadLineeOpenSessionResultComplete(CaseSessionResult sessionResult)
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
        public void DeadLineCompanyCaseStartOnDocument(Document document)
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
                var document = repo.AllReadonly<Document>()
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
                                   x.DeadlineTypeId == NomenclatureConstants.DeadlineType.CompanyCaseChange ))
                      .ToList();
            foreach (var deadline in deadlines)
            {
                var sessionResult = repo.AllReadonly<CaseSessionResult>()
                         .Include(x => x.CaseSession)
                         .Where(x => x.IsActive)
                         .Where(x => x.CaseSession.CaseId == companyCase.Id && 
                                     x.CaseSession.DateFrom >= deadline.StartDate.Date)
                         .OrderBy(x => x.Id)
                         .FirstOrDefault();

                if (sessionResult != null)
                {
                    deadline.DateComplete = sessionResult.CaseSession?.DateFrom ?? DateTime.Now;
                    deadline.CaseSessionResultId = sessionResult.Id;
                    result.Add(deadline);
                }
            }
            return result;
        }
        #endregion CompanyCase
        public IQueryable<CaseDeadLineVM> CaseDeadLineSelect(CaseDeadLineFilterVM filter)
        {
            var users = repo.AllReadonly<CaseSessionMeetingUser>()
                           .Include(x => x.SecretaryUser)
                           .ThenInclude(x => x.LawUnit);

            var deadlines = repo.AllReadonly<CaseDeadline>()
                       .Where(x => x.CourtId == userContext.CourtId)
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
                deadlines = deadlines.Where(x => x.Case.CaseLawUnits.Any(l => l.LawUnitId == filter.LawUnitId && l.CaseSessionId == null && l.DateTo == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter));
            if (!string.IsNullOrEmpty(filter.RegNumber))
                deadlines = deadlines.Where(x => x.Case.RegNumber.EndsWith(filter.RegNumber));
            if (filter.DeadlineTypeId > 0)
                deadlines = deadlines.Where(x => x.DeadlineTypeId == filter.DeadlineTypeId);
            return deadlines.Select(x => new CaseDeadLineVM()
            {
                Id = x.Id,
                CaseId = x.CaseId,
                CaseInfo = x.Case.RegNumber + "/" + x.Case.RegDate.ToString(FormattingConstant.NormalDateFormat) + " " + (x.Case.CaseType.Code ?? ""),
                LawUnitName = string.Join("<br>", x.Case.CaseLawUnits.Where(l => l.CaseSessionId == null && l.DateTo == null && l.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).Select(n => n.LawUnit != null ? n.LawUnit.FullName : "")),
                SecretaryName = x.DeadlineTypeId != NomenclatureConstants.DeadlineType.OpenSessionResult ? "" :
                                           string.Join("<br>", users.Where(u => u.CaseSessionMeeting.CaseSessionId == x.SourceId).Select(s => s.SecretaryUser.LawUnit.FullName)),
                DeadlineGroup = x.DeadlineGroup.Label,
                DeadlineType = x.DeadlineType.Label,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                DateComplete = x.DateComplete,
                SourceUrl = GetTaskObjectUrl(x.SourceType, x.SourceId)
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
    }
}
