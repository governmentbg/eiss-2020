// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class WorkNotificationService : BaseService, IWorkNotificationService
    {
        private readonly IUrlHelper urlHelper;
        private readonly ICaseLawUnitService caseLawUnitService;
        public WorkNotificationService(
            ILogger<WorkNotificationService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            IUrlHelper _url,
            ICaseLawUnitService _caseLawUnitService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            urlHelper = _url;
            caseLawUnitService = _caseLawUnitService;
        }

        public List<SelectListItem> GetDDL_WorkNotificationTypes(int sourceType)
        {
            var model = repo.AllReadonly<WorkNotificationType>();
            return model.ToSelectList(true);
        }
        public IQueryable<WorkNotification> SelectWorkNotifications(WorkNotificationFilterVM filterData)
        {
            if (filterData.DateFrom != null)
                filterData.DateFrom = filterData.DateFrom?.Date;
            if (filterData.DateTo != null)
            {
                filterData.DateTo = filterData.DateTo?.Date;
                filterData.DateTo = filterData.DateTo?.AddHours(23);
                filterData.DateTo = filterData.DateTo?.AddMinutes(59);
                filterData.DateTo = filterData.DateTo?.AddSeconds(59);
                filterData.DateTo = filterData.DateTo?.AddMilliseconds(999);
            }

            var result = repo.AllReadonly<WorkNotification>()
                            .Include(x => x.WorkNotificationType)
                            .Include(x => x.User)
                            .ThenInclude(x => x.LawUnit)
                            .Include(x => x.FromUser)
                            .ThenInclude(x => x.LawUnit)
                            .Include(x => x.Court)
                            .Include(x => x.FromCourt)
                            .Where(x => x.CourtId == filterData.CourtId)
                            .Where(x => x.UserId == filterData.UserId)
                            .Where(x => x.DateCreated.Date <= filterData.DateCreate)
                            .Where(x => filterData.WorkNotificationTypeId <= 0 || filterData.WorkNotificationTypeId == x.WorkNotificationTypeId)
                            .Where(x => filterData.SourceType <= 0 || x.SourceType == filterData.SourceType)
                            .Where(x => filterData.SourceId <= 0 || x.SourceId == filterData.SourceId)
                            .Where(x => (filterData.ReadTypeId != WorkNotificationFilterVM.ReadTypeRead && x.DateRead == null) ||
                                      (filterData.ReadTypeId != WorkNotificationFilterVM.ReadTypeUnRead && x.DateRead >= filterData.DateFrom && x.DateRead <= filterData.DateTo))
                            .ToList();
            foreach (var item in result)
            {
                item.SourceUrl = GetTaskObjectUrl(item.SourceType, item.SourceId, item.WorkNotificationTypeId);
            }
            return result.AsQueryable();
        }
        public bool SaveWorkNotification(WorkNotification workNotification)
        {
            try
            {
                repo.Add(workNotification);
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }
        public bool SaveWorkNotificationRead(long id)
        {
            try
            {
                var notification = repo.GetById<WorkNotification>(id);
                notification.DateRead = DateTime.Now;
                repo.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        public List<SelectListItem> ReadTypeId_SelectDDL()
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem { Text = "Непрочетени", Value = WorkNotificationFilterVM.ReadTypeUnRead.ToString() });
            result.Add(new SelectListItem { Text = "Всички", Value = WorkNotificationFilterVM.ReadTypeAll.ToString() });
            result.Add(new SelectListItem { Text = "Прочетени", Value = WorkNotificationFilterVM.ReadTypeRead.ToString() });
            return result;
        }
        public WorkNotificationFilterVM MakeDefaultFilter()
        {
            WorkNotificationFilterVM model = new WorkNotificationFilterVM();
            model.ReadTypeId = WorkNotificationFilterVM.ReadTypeUnRead;
            model.DateFrom = DateTime.Now;
            model.DateCreate = DateTime.Now;
            model.DateTo = DateTime.Now;
            model.UserId = userContext.UserId;
            model.CourtId = userContext.CourtId;
            return model;
        }
        private Case GetCaseByNotification(CaseNotification caseNotification)
        {
            var aCase = caseNotification.Case;
            if (aCase == null)
                aCase = repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseNotification.CaseId)
                            .FirstOrDefault();
            return aCase;
        }
        public int[] NotificationDelivered()
        {
            return new int[]
            {
               NomenclatureConstants.NotificationState.Delivered,
               NomenclatureConstants.NotificationState.Delivered47,
               NomenclatureConstants.NotificationState.Delivered50,
               NomenclatureConstants.NotificationState.Delivered51
            };
        }
        private WorkNotification NewWorkNotificationDeliveredList(CaseNotification caseNotification)
        {
            var userId = GetJudgeUserId(caseNotification.CaseId);
            if (userId == null)
                return null;
            var workNotification = repo.AllReadonly<WorkNotification>()
                                        .Where(x => x.SourceType == SourceTypeSelectVM.CaseSession)
                                        .Where(x => x.SourceId == caseNotification.CaseSessionId)
                                        .Where(x => x.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.ListDelivered)
                                        .FirstOrDefault();
            if (workNotification != null)
                return null;

            var aCase = GetCaseByNotification(caseNotification);
            var caseSession = repo.AllReadonly<CaseSession>()
                           .Where(x => x.Id == caseNotification.CaseSessionId)
                           .FirstOrDefault();

            var personList = repo.AllReadonly<CaseSessionNotificationList>()
                                 .Where(x => x.CaseSessionId == caseNotification.CaseSessionId)
                                 .ToList();
            var delivered = NotificationDelivered();
            var notifications = repo.AllReadonly<CaseNotification>()
                                 .Where(x => delivered.Contains(x.NotificationStateId))
                                 .Where(x => x.CaseSessionId == caseNotification.CaseSessionId)
                                 .ToList();
            if (personList.Any(x => x.CasePersonId != null && !notifications.Any(n => n.CasePersonId == x.CasePersonId)))
                return null;
            if (personList.Any(x => x.CaseLawUnitId != null && !notifications.Any(n => n.CaseLawUnitId == x.CaseLawUnitId)))
                return null;
            var model = new WorkNotification();
            model.SourceType = SourceTypeSelectVM.CaseSession;
            model.SourceId = caseNotification.CaseSessionId ?? 0;
            model.WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.ListDelivered;
            model.Title = $"Известията от списъка за сесия от {caseSession.DateFrom.ToString(FormattingConstant.NormalDateFormat)}  са доставени";
            model.Description = $"Известията по дело: {aCase.RegNumber}/{aCase.RegDate.ToString(FormattingConstant.NormalDateFormat)} за сесията от {caseSession.DateFrom.ToString(FormattingConstant.NormalDateFormat)} " +
                                " са доставени";
            model.LinkLabel = "Списък за " + caseSession.DateFrom.ToString(FormattingConstant.NormalDateFormat);
            model.CourtId = aCase.CourtId;
            model.FromCourtId = userContext.CourtId;
            model.FromUserId = userContext.UserId;
            model.DateCreated = DateTime.Now;
            model.UserId = userId;
            return model;
        }
        public string GetJudgeUserId(int caseId)
        {
            var caseLawUnitsActive = caseLawUnitService.CaseLawUnit_Select(caseId, null).ToList();
            var judgeRep = caseLawUnitsActive.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
            if (judgeRep == null)
                return null;
            return GetUserIdByLawUnitId(judgeRep.LawUnitId);
        }
        private List<string> GetSessionMeetingSecretaryUserId(int caseSessionId)
        {
            var users = repo.AllReadonly<CaseSessionMeetingUser>()
                            .Where(x => x.CaseSessionMeeting.CaseSessionId == caseSessionId)
                            .Select(x => x.SecretaryUserId)
                            .Distinct()
                            .ToList();
            return users;
        }
        private WorkNotification NewWorkNotificationUnDelivered(CaseNotification caseNotification)
        {
            var userId = GetJudgeUserId(caseNotification.CaseId);
            if (userId == null)
                return null;
            var workNotification = repo.AllReadonly<WorkNotification>()
                                       .Where(x => x.SourceType == SourceTypeSelectVM.CaseNotification)
                                       .Where(x => x.SourceId == caseNotification.Id)
                                       .Where(x => x.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.UnDeliveredNotification)
                                       .FirstOrDefault();
            if (workNotification != null)
                return null;

            var aCase = GetCaseByNotification(caseNotification);
            var notificationType = caseNotification.NotificationType;
            if (notificationType == null)
                notificationType = repo.AllReadonly<NotificationType>()
                                       .Where(x => x.Id == (caseNotification.NotificationTypeId ?? 0))
                                       .FirstOrDefault();

            var model = new WorkNotification();
            model.SourceType = SourceTypeSelectVM.CaseNotification;
            model.SourceId = caseNotification.Id;
            model.WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.UnDeliveredNotification;
            model.Title = $"Върнато известие {caseNotification.RegNumber} ";
            model.Description = $"{notificationType.Label}  {caseNotification.RegNumber}/{caseNotification.RegDate.ToString(FormattingConstant.NormalDateFormat)}" +
                                " е върнато в цялост";
            model.LinkLabel = notificationType.Label;
            model.CourtId = aCase.CourtId;
            model.FromCourtId = userContext.CourtId;
            model.FromUserId = userContext.UserId;
            model.DateCreated = DateTime.Now;
            model.UserId = userId;
            return model;
        }
        public WorkNotification NewWorkNotification(CaseNotification caseNotification)
        {
            if (caseNotification.NotificationStateId == NomenclatureConstants.NotificationState.UnDelivered)
                return NewWorkNotificationUnDelivered(caseNotification);

            if (caseNotification.CaseSessionId > 0 && caseNotification.CaseSessionActId == null && caseNotification.NotificationStateId != NomenclatureConstants.NotificationState.UnDelivered)
                return NewWorkNotificationDeliveredList(caseNotification);

            return null;
        }
        public string GetTaskObjectUrl(int sourceType, long sourceId, int workNotificationTypeId)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.Case:
                    return urlHelper.Action("CasePreview", "Case", new { id = sourceId });
                case SourceTypeSelectVM.CaseNotification:
                    return urlHelper.Action("Edit", "CaseNotification", new { id = sourceId });
                case SourceTypeSelectVM.CaseSession:
                    {
                        if (workNotificationTypeId == NomenclatureConstants.WorkNotificationType.DeadLine)
                            return urlHelper.Action("Preview", "CaseSession", new { id = sourceId, tab = "tabname" }).Replace("tabname", "#tabSessionMainData", StringComparison.InvariantCultureIgnoreCase);
                        else
                            return urlHelper.Action("Preview", "CaseSession", new { id = sourceId, tab = "tabname" }).Replace("tabname", "#tabPersonNotification", StringComparison.InvariantCultureIgnoreCase);
                    }
                default:
                    return string.Empty;
            }
        }
        public WorkNotification NewWorkNotification(CaseDeadline caseDeadline)
        {
            var userId = GetJudgeUserId(caseDeadline.CaseId);
            if (userId == null)
                return null;
            var workNotification = repo.AllReadonly<WorkNotification>()
                                                   .Where(x => x.CaseDeadlineId == caseDeadline.Id &&
                                                               x.UserId == userId)
                                                   .FirstOrDefault();
            if (workNotification != null)
            {
                if (caseDeadline.DateComplete != null || caseDeadline.DateExpired != null)
                {
                    if (workNotification.DateExpired == null)
                    {
                        workNotification.DateExpired = caseDeadline.DateExpired ?? caseDeadline.DateComplete;
                        workNotification.UserExpiredId = userContext.UserId;
                        return workNotification;
                    }
                }
                return null;
            }
            var deadlineType = repo.AllReadonly<DeadlineType>()
                                   .Where(x => x.Id == caseDeadline.DeadlineTypeId)
                                   .FirstOrDefault();
            deadlineType = deadlineType ?? new DeadlineType();
            var aCase = repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseDeadline.CaseId)
                            .FirstOrDefault();
            if (aCase == null)
                return null;
            var model = new WorkNotification();
            model.SourceType = caseDeadline.SourceType;
            model.SourceId = caseDeadline.SourceId;
            model.WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.DeadLine;
            model.CaseDeadlineId = caseDeadline.Id;
            model.Title = $"Изтичащ срок по дело {aCase.RegNumber}";
            model.Description = $"{deadlineType.Label} стартиран {caseDeadline.StartDate.ToString(FormattingConstant.NormalDateFormat)} изтичащ {caseDeadline.EndDate.ToString(FormattingConstant.NormalDateFormat)}";
            model.LinkLabel = "Срок";
            model.CourtId = aCase.CourtId;
            model.FromCourtId = userContext.CourtId;
            model.FromUserId = userContext.UserId;
            model.DateCreated = caseDeadline.EndDate.AddDays(-5);
            model.UserId = userId;
            return model;
        }
        public List<WorkNotification> NewWorkNotificationSecretary(CaseDeadline caseDeadline)
        {
            var result = new List<WorkNotification>();
            if (caseDeadline.DeadlineTypeId != NomenclatureConstants.DeadlineType.OpenSessionResult)
                return result;
            var deadlineType = repo.AllReadonly<DeadlineType>()
                                   .Where(x => x.Id == caseDeadline.DeadlineTypeId)
                                   .FirstOrDefault();
            if (deadlineType == null)
                return result;

            deadlineType = deadlineType ?? new DeadlineType();
            var aCase = repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseDeadline.CaseId)
                            .FirstOrDefault();
            if (aCase == null)
                return result;
            int caseSessionId = (int)caseDeadline.SourceId;
            var users = GetSessionMeetingSecretaryUserId(caseSessionId);
            foreach (string userId in users)
            {
                var workNotification = repo.AllReadonly<WorkNotification>()
                                                       .Where(x => x.CaseDeadlineId == caseDeadline.Id && x.UserId == userId)
                                                       .FirstOrDefault();
                if (workNotification != null)
                {
                    if (caseDeadline.DateComplete != null || caseDeadline.DateExpired != null)
                    {
                        if (workNotification.DateExpired == null)
                        {
                            workNotification.DateExpired = caseDeadline.DateExpired ?? caseDeadline.DateComplete;
                            workNotification.UserExpiredId = userContext.UserId;
                           result.Add(workNotification);
                        }
                    }
                    continue;
                }

                var model = new WorkNotification();
                model.SourceType = caseDeadline.SourceType;
                model.SourceId = caseDeadline.SourceId;
                model.WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.DeadLine;
                model.CaseDeadlineId = caseDeadline.Id;
                model.Title = $"Изтичащ срок по дело {aCase.RegNumber}";
                model.Description = $"{deadlineType.Label} стартиран {caseDeadline.StartDate.ToString(FormattingConstant.NormalDateFormat)} изтичащ {caseDeadline.EndDate.ToString(FormattingConstant.NormalDateFormat)}";
                model.LinkLabel = "Срок";
                model.CourtId = aCase.CourtId;
                model.FromCourtId = userContext.CourtId;
                model.FromUserId = userContext.UserId;
                model.DateCreated = caseDeadline.EndDate.AddDays(-5);
                model.UserId = userId;
                result.Add(model);
            }
            return result;
        }
        public WorkNotification NewWorkNotification(CaseLawUnit model)
        {
            string userId = GetUserIdByLawUnitId(model.LawUnitId);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            if (model.Case == null)
            {
                model.Case = repo.GetById<Case>(model.CaseId);
            }
            var info = repo.AllReadonly<CaseLawUnit>()
                                .Where(x => x.CaseId == model.CaseId && x.LawUnitId == model.LawUnitId)
                                .Where(x => x.CaseSessionId == null)
                                .Select(x => new
                                {
                                    FullName = x.LawUnit.FullName,
                                    JudgeRole = x.JudgeRole.Label,
                                    DateFrom = x.DateFrom
                                }).FirstOrDefault();
            var workNotification = new WorkNotification();
            workNotification.SourceType = SourceTypeSelectVM.Case;
            workNotification.SourceId = model.CaseId;
            workNotification.WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.NewCase;
            workNotification.Title = $"Разпределено дело {model.Case.RegNumber}";
            workNotification.Description = $"{info.FullName}, имате разпределено дело {model.Case.RegNumber} като {info.JudgeRole}.";
            workNotification.LinkLabel = "Дело";
            workNotification.CourtId = model.Case.CourtId;
            workNotification.FromCourtId = userContext.CourtId;
            workNotification.FromUserId = userContext.UserId;
            workNotification.DateCreated = info.DateFrom;
            workNotification.UserId = userId;
            return workNotification;
        }

    }
}
