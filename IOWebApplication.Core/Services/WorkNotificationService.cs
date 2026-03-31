using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Base;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Documents;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Data.Models.Money;
using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Case;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static NPOI.SS.Format.CellNumberFormatter;

namespace IOWebApplication.Core.Services
{
    public class WorkNotificationService : BaseService, IWorkNotificationService
    {
        private readonly IWorkingDaysService workingDaysService;


        public WorkNotificationService(ILogger<WorkNotificationService> _logger,
                                       IRepository _repo,
                                       IUserContext _userContext,
                                       IWorkingDaysService _workingDaysService)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            workingDaysService = _workingDaysService;
        }

        public List<SelectListItem> GetDDL_WorkNotificationTypes(int sourceType)
        {
            var model = repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.WorkNotificationType>().OrderBy(x => x.Label);
            return model.ToSelectList(true, false, false);
        }

        /// <summary>
        /// Метод извличащ данни за нотификации
        /// </summary>
        /// <param name="filterData">Филтър попълнен от потребител</param>
        /// <returns></returns>
        public IQueryable<WorkNotificationListVM> SelectWorkNotifications(WorkNotificationFilterVM filterData)
        {
            filterData.DateFrom = filterData.DateFrom.ForceStartDate();
            filterData.DateTo = filterData.DateTo.ForceEndDate();

            DateTime dateNow = DateTime.Now;

            Expression<Func<WorkNotification, bool>> dateCreatedWhere = x => x.DateCreated.Date <= (filterData.DateCreate ?? dateNow).Date && x.DateCreated <= dateNow;

            Expression<Func<WorkNotification, bool>> dateReadWhere = x => true;
            if (filterData.ReadTypeId != WorkNotificationFilterVM.ReadTypeAll)
            {
                if (filterData.ReadTypeId == WorkNotificationFilterVM.ReadTypeRead)
                    dateReadWhere = x => x.DateRead >= filterData.DateFrom && x.DateRead <= filterData.DateTo;

                if (filterData.ReadTypeId == WorkNotificationFilterVM.ReadTypeUnRead)
                    dateReadWhere = x => x.DateRead == null;
            }

            Expression<Func<WorkNotification, bool>> workNotificationIdWhere = x => true;
            if (filterData.Id != null)
            {
                workNotificationIdWhere = x => x.Id == filterData.Id;
                dateCreatedWhere = x => true;
                dateReadWhere = x => true;
            }

            Expression<Func<WorkNotification, bool>> workNotificationTypeIdWhere = x => true;
            if (filterData.WorkNotificationTypeId > 0)
                workNotificationTypeIdWhere = x => x.WorkNotificationTypeId == filterData.WorkNotificationTypeId;

            Expression<Func<WorkNotification, bool>> sourceTypeWhere = x => true;
            if (filterData.SourceType > 0)
                sourceTypeWhere = x => x.SourceType == filterData.SourceType;

            Expression<Func<WorkNotification, bool>> sourceIdWhere = x => true;
            if (filterData.SourceId > 0)
                sourceIdWhere = x => x.SourceId == filterData.SourceId;

            Expression<Func<WorkNotification, bool>> notificationKindWhere = x => true;
            if (filterData.NotificationKind != null && filterData.NotificationKind > 0)
                notificationKindWhere = x => x.NotificationKind == filterData.NotificationKind;

            Expression<Func<WorkNotification, bool>> userCourtDepartmentIdWhere = x => true;
            if (filterData.UserCourtDepartmentId != null && filterData.UserCourtDepartmentId > 0)
                userCourtDepartmentIdWhere = x => x.UserCourtDepartmentId == filterData.UserCourtDepartmentId;

            Expression<Func<WorkNotification, bool>> caseRegNumWhere = x => true;
            if (!string.IsNullOrEmpty(filterData.CaseRegNumber))
                caseRegNumWhere = x => EF.Functions.ILike(x.Case.RegNumber, filterData.CaseRegNumber.ToPaternSearch());

            return repo.AllReadonly<WorkNotification>()
                       .Where(workNotificationIdWhere)
                       .Where(workNotificationTypeIdWhere)
                       .Where(sourceTypeWhere)
                       .Where(sourceIdWhere)
                       .Where(dateCreatedWhere)
                       .Where(dateReadWhere)
                       .Where(notificationKindWhere)
                       .Where(userCourtDepartmentIdWhere)
                       .Where(caseRegNumWhere)
                       .Where(x => x.DateExpired == null &&
                                   x.DateTurnOff == null &&
                                   x.CourtId == filterData.CourtId &&
                                   x.UserId == filterData.UserId)
                       .Select(x => new WorkNotificationListVM()
                       {
                           Id = x.Id,
                           DateCreated = x.DateCreated,
                           Title = x.Title,
                           DateRead = x.DateRead,
                           Description = x.Description,
                           IsUnRead = x.DateRead == null,
                           IsEdit = x.NotificationKind == 2,
                           NotificationKind = x.NotificationKind,
                           WorkNotificationTypeId = x.WorkNotificationTypeId,
                           WorkNotificationTypeLabel = x.WorkNotificationType.Label
                       });
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
        public WorkNotification SaveWorkNotificationRead(long id)
        {
            try
            {
                var notification = repo.GetById<WorkNotification>(id);
                if (notification.DateRead == null && notification.NotificationKind != 2)
                {
                    notification.DateRead = DateTime.Now;
                    repo.SaveChanges();
                }
                return notification;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
            }
        }

        public WorkNotification SaveWorkNotificationReadAll(long id)
        {
            try
            {
                var notification = repo.GetById<WorkNotification>(id);
                if (notification.DateRead == null)
                {
                    notification.DateRead = DateTime.Now;
                    repo.SaveChanges();
                }
                return notification;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return null;
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
                                 .Where(x => x.CaseSessionId == caseNotification.CaseSessionId && x.DateExpired == null)
                                 .ToList();
            var delivered = NomenclatureConstants.NotificationState.NotificationDelivered();
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
            model.CaseId = aCase.Id;
            model.FromCourtId = (userContext.CourtId) > 0 ? userContext.CourtId : aCase.CourtId;
            model.FromUserId = userContext.UserId;
            model.DateCreated = DateTime.Now;
            model.UserId = userId;
            return model;
        }

        /// <summary>
        /// Метод връщащ идентификатор на юзер на лице
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSesionId">Идентификатор на заседание</param>
        /// <returns></returns>
        public string GetJudgeUserId(int caseId, int? caseSesionId = null)
        {
            CaseLawUnitSmallVM judge = GetOnlyJudgeReporterFastProcess(caseId, caseSesionId);
            return (judge == null) ? string.Empty : judge.LawUnitUserId;

        }

        /// <summary>
        /// Метод връщащ идентификатор на юзер на лице
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSesionId">Идентификатор на заседание</param>
        /// <returns></returns>
        public async Task<string> GetJudgeUserIdAsync(int caseId, int? caseSesionId = null)
        {
            CaseLawUnitSmallVM? judge = await GetOnlyJudgeReporterFastProcessAsync(caseId, caseSesionId);
            return (judge == null) ? string.Empty : judge.LawUnitUserId;

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
            if (!repo.AllReadonly<Case>()
                     .Where(x => x.Id == caseNotification.CaseId &&
                                 !(x.IsFastProcess ?? false))
                     .Any())
                return null;

            var userId = GetJudgeUserId(caseNotification.CaseId);
            if (string.IsNullOrEmpty(userId))
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
                notificationType = repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.NotificationType>()
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
            model.CaseId = aCase.Id;
            model.FromCourtId = (userContext.CourtId) > 0 ? userContext.CourtId : aCase.CourtId;
            model.FromUserId = userContext.UserId;
            model.DateCreated = DateTime.Now;
            model.UserId = userId;
            return model;
        }
        public async Task<WorkNotification> NewWorkNotification(int notificationId, int notificationStateId)
        {
            if (notificationStateId == NomenclatureConstants.NotificationState.UnDelivered)
            {
                var caseNotification = await repo.AllReadonly<CaseNotification>()
                                           .Where(x => x.Id == notificationId)
                                           .FirstAsync();
                return NewWorkNotificationUnDelivered(caseNotification);
            }

            if (NomenclatureConstants.NotificationState.NotificationDelivered().Contains(notificationStateId))
            {
                var caseNotification = await repo.AllReadonly<CaseNotification>()
                                           .Where(x => x.Id == notificationId)
                                           .FirstAsync();
                if (caseNotification.CaseSessionId > 0 && caseNotification.CaseSessionActId == null)
                {
                    return NewWorkNotificationDeliveredList(caseNotification);
                }
            }
            return null;
        }

        /// <summary>
        /// Връща userId
        /// </summary>
        /// <param name="caseDeadline">Попълнен обект за срок</param>
        /// <returns></returns>
        private string GetUserIdForCaseDeadline(CaseDeadline caseDeadline)
        {
            string userId = null;
            if (caseDeadline.SourceType == SourceTypeSelectVM.CaseSession)
                userId = GetJudgeUserId(caseDeadline.CaseId, (int)caseDeadline.SourceId);

            if (string.IsNullOrEmpty(userId))
                userId = GetJudgeUserId(caseDeadline.CaseId);

            return userId;
        }

        /// <summary>
        /// Метод за запис на срок и нотификация
        /// </summary>
        /// <param name="caseDeadline">Попълнен обект от тип CaseDeadline</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> NewWorkNotification(CaseDeadline caseDeadline)
        {
            var aCase = repo.AllReadonly<Case>()
                            .Where(x => x.Id == caseDeadline.CaseId)
                            .Select(x => new { x.RegNumber, x.CourtId })
                            .FirstOrDefault();

            if (aCase == null)
                return null;

            List<CaseLawUnitSmallVM> caseLawUnits = [];

            if (caseDeadline.NotificationKind == 2)
                caseLawUnits = await GetCaseLawUnitAllFastProcess(0, aCase.CourtId, caseDeadline.CaseId, null, DateTime.Now);
            else
                caseLawUnits = await GetCaseLawUnitAllFastProcess(3, aCase.CourtId, caseDeadline.CaseId, caseDeadline.SourceType == SourceTypeSelectVM.CaseSession ? (int)caseDeadline.SourceId : null, DateTime.Now, false);

            if (caseLawUnits == null || caseLawUnits.Count < 1)
                return null;

            List<WorkNotification> workNotifications = await repo.AllReadonly<WorkNotification>()
                                                                 .Where(x => x.CaseDeadlineId == caseDeadline.Id &&
                                                                             caseLawUnits.Select(u => u.LawUnitUserId).Contains(x.UserId) &&
                                                                             x.DateExpired == null)
                                                                 .ToListAsync() ?? [];

            if (workNotifications.Count > 0)
            {
                if (caseDeadline.DateComplete != null || caseDeadline.DateExpired != null)
                {
                    foreach (var workNotification in workNotifications)
                    {
                        workNotification.DateExpired = caseDeadline.DateExpired ?? caseDeadline.DateComplete;
                        workNotification.UserExpiredId = userContext.UserId;
                    }

                    return workNotifications;
                }

                return null;
            }

            Infrastructure.Data.Models.Nomenclatures.DeadlineType deadlineType = repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.DeadlineType>()
                                                                                     .Where(x => x.Id == caseDeadline.DeadlineTypeId)
                                                                                     .FirstOrDefault() ?? new();

            foreach (var caseLawUnit in caseLawUnits)
            {
                workNotifications.Add(new WorkNotification()
                {
                    SourceType = caseDeadline.SourceType,
                    SourceId = caseDeadline.SourceId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.DeadLine,
                    CaseDeadlineId = caseDeadline.Id,
                    Title = $"Изтичащ срок по дело {aCase.RegNumber}",
                    Description = $"{deadlineType.Label} стартиран {caseDeadline.StartDate.ToString(FormattingConstant.NormalDateFormat)} изтичащ {caseDeadline.EndDate.ToString(FormattingConstant.NormalDateFormat)}",
                    LinkLabel = "Срок",
                    CourtId = aCase.CourtId,
                    CaseId = caseDeadline.CaseId,
                    FromCourtId = (userContext.CourtId > 0) ? userContext.CourtId : aCase.CourtId,
                    FromUserId = userContext.UserId,
                    DateCreated = caseDeadline.StartDate > caseDeadline.EndDate.AddDays((deadlineType.ShowNotificationBeforEndDeadlineDays ?? 0) * -1) ? caseDeadline.StartDate : caseDeadline.EndDate.AddDays((deadlineType.ShowNotificationBeforEndDeadlineDays ?? 0) * -1),
                    UserId = caseLawUnit.LawUnitUserId,
                    NotificationKind = caseDeadline.NotificationKind,
                });
            }

            return workNotifications;
        }

        public List<WorkNotification> NewWorkNotificationSecretary(CaseDeadline caseDeadline)
        {
            var result = new List<WorkNotification>();
            if (caseDeadline.DeadlineTypeId != NomenclatureConstants.DeadlineType.OpenSessionResult)
                return result;
            var deadlineType = repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.DeadlineType>()
                                   .Where(x => x.Id == caseDeadline.DeadlineTypeId)
                                   .FirstOrDefault();
            if (deadlineType == null)
                return result;

            deadlineType = deadlineType ?? new Infrastructure.Data.Models.Nomenclatures.DeadlineType();
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
                model.CaseId = aCase.Id;
                model.FromCourtId = (userContext.CourtId) > 0 ? userContext.CourtId : aCase.CourtId;
                model.FromUserId = userContext.UserId;
                model.DateCreated = caseDeadline.EndDate.AddDays(-5);
                if (caseDeadline.StartDate > model.DateCreated)
                    model.DateCreated = caseDeadline.StartDate;
                model.UserId = userId;
                model.NotificationKind = caseDeadline.NotificationKind;
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
                               x.LawUnit.FullName,
                               JudgeRole = x.JudgeRole.Label,
                               x.DateFrom
                           })
                           .FirstOrDefault();

            var workNotification = new WorkNotification();
            workNotification.SourceType = SourceTypeSelectVM.Case;
            workNotification.SourceId = model.CaseId;
            workNotification.WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.NewCase;
            workNotification.Title = $"Разпределено дело {model.Case.RegNumber}";
            workNotification.Description = $"{info.FullName}, имате разпределено дело {model.Case.RegNumber} като {info.JudgeRole}.";
            workNotification.LinkLabel = "Дело";
            workNotification.CourtId = model.Case.CourtId;
            workNotification.CaseId = model.CaseId;
            workNotification.FromCourtId = (userContext.CourtId) > 0 ? userContext.CourtId : model.Case.CourtId;
            workNotification.FromUserId = userContext.UserId;
            workNotification.DateCreated = info.DateFrom;
            workNotification.UserId = userId;
            return workNotification;
        }

        public WorkNotification NewWorkNotification(CaseLawyerHelpAssignedLawyer model)
        {

            var judgeReporter = repo.AllReadonly<CaseLawyerHelp>()
                                    .Include(x => x.Case)
                                    .ThenInclude(x => x.CaseLawUnits)
                                    .ThenInclude(x => x.LawUnit)
                                    .Where(x => x.Id == model.CaseLawyerHelpId)
                                    .SelectMany(x => x.Case.CaseLawUnits)
                                    .Where(x => x.DateTo == null && x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter)
                                    .Select(x => new
                                    {
                                        x.Id,
                                        x.LawUnitUserId,
                                        x.LawUnit.FullName
                                    })
                                    .FirstOrDefault();

            if (judgeReporter == null)
            {
                return null;
            }

            var caseInfo = repo.AllReadonly<CaseLawyerHelp>()
                               .Include(x => x.Case)
                               .ThenInclude(x => x.CaseType)
                               .Where(x => x.Id == model.CaseLawyerHelpId)
                               .Select(x => new
                               {
                                   x.CaseId,
                                   x.Case.CourtId,
                                   CaseType = x.Case.CaseType.Code,
                                   CaseNumber = x.Case.ShortNumber,
                                   CaseYear = x.Case.RegDate.Year
                               })
                               .FirstOrDefault();

            if (caseInfo == null)
            {
                return null;
            }
            var workNotification = new WorkNotification();
            workNotification.SourceType = SourceTypeSelectVM.CaseLawyerHelp;
            workNotification.SourceId = model.CaseLawyerHelpId;
            workNotification.WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.CaseLawyerHelpAssigned;
            workNotification.Title = $"Посочен адвокат";
            workNotification.Description = $"{judgeReporter.FullName}, имате посочен адвокат по искане за правна помощ към дело {caseInfo.CaseType} {caseInfo.CaseNumber}/{caseInfo.CaseYear}.";
            workNotification.LinkLabel = "Адвокат";
            workNotification.CourtId = caseInfo.CourtId;
            workNotification.FromCourtId = caseInfo.CourtId;
            workNotification.CaseId = caseInfo.CaseId;
            //workNotification.FromUserId = userContext.UserId;
            workNotification.DateCreated = model.DateReturned;
            workNotification.UserId = judgeReporter.LawUnitUserId;
            return workNotification;
        }

        /// <summary>
        /// Метод променящ дата на визуализация на нотификацията
        /// </summary>
        /// <param name="model">Модел попълнен от потребител</param>
        /// <returns></returns>
        public async Task<bool> EditDateCreatedWNFastProcess(WorkNotificationEditDateVM model)
        {
            try
            {
                WorkNotification notification = await repo.All<WorkNotification>()
                                                          .Where(x => x.Id == model.NotificationId)
                                                          .FirstAsync();

                notification.DateCreated = notification.DateCreated.AddDays(model.AddDays);
                notification.DateRead = null;

                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при промяна на дата на визуализация на нотификацията id: {model.NotificationId}");
                return false;
            }
        }

        #region Нотификации за дела по чл. 410 ГПК или чл. 417 ГПК

        #region Load Data CaseInfoFastProcessVM

        /// <summary>
        /// Метод извличащ всички идентификатори на предходни дела от миграциите
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<List<int>> GetPriorCaseIds(int caseId)
        {
            return await repo.AllReadonly<CaseMigration>()
                             .Where(x => x.CaseId == caseId)
                             .Select(x => x.PriorCaseId)
                             .Distinct()
                             .ToListAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataNewCaseFastProcess(int caseId)
        {
            return await repo.AllReadonly<Case>()
                             .Where(c => c.Id == caseId)
                             .Where(c => c.IsFastProcess ?? false)
                             .Select(c => new CaseInfoFastProcessVM()
                             {
                                 CaseCourtId = c.CourtId,
                                 CaseTypeLabel = c.CaseType.Label,
                                 CaseRegNumber = c.RegNumber,
                                 CaseYear = c.RegDate.Year,
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <param name="caseSessionResultId">Идентификатор на резултат</param>
        /// <returns></returns>
        private async Task<List<CaseInfoFastProcessVM>> GetDataTakingActionN3(int? caseSessionActId, int? caseSessionResultId)
        {
            if (((caseSessionActId ?? 0) < 1) && ((caseSessionResultId ?? 0) < 1))
                return [];

            Expression<Func<CaseSessionAct, bool>> caseSessionActIdWhere = a => true;
            if ((caseSessionActId ?? 0) > 0)
                caseSessionActIdWhere = a => a.Id == caseSessionActId &&
                                             ((a.ActTypeId == NomenclatureConstants.ActType.CommandmentForExec) ||
                                              (NomenclatureConstants.ActType.NotificationN3.Contains(a.ActTypeId) && a.CaseSession
                                                                                                                      .CaseSessionResults
                                                                                                                      .Any(r => r.DateExpired == null &&
                                                                                                                                NomenclatureConstants.CaseSessionResult.ResultsStartNotificationN3.Contains(r.SessionResultId))));
            Expression<Func<CaseSessionAct, bool>> caseSessionResultIdWhere = a => true;
            if ((caseSessionResultId ?? 0) > 0)
                caseSessionResultIdWhere = a => NomenclatureConstants.ActType.NotificationN3.Contains(a.ActTypeId) &&
                                                a.CaseSession
                                                 .CaseSessionResults
                                                 .Any(r => r.Id == caseSessionResultId &&
                                                           r.DateExpired == null &&
                                                           NomenclatureConstants.CaseSessionResult.ResultsStartNotificationN3.Contains(r.SessionResultId));

            return await repo.AllReadonly<CaseSessionAct>()
                             .Where(a => a.CaseId != null)
                             .Where(a => a.Case.IsFastProcess ?? false)
                             .Where(a => a.ActDeclaredDate != null)
                             .Where(caseSessionActIdWhere)
                             .Where(caseSessionResultIdWhere)
                             .Select(a => new CaseInfoFastProcessVM()
                             {
                                 CaseId = a.CaseId ?? 0,
                                 CaseCourtId = a.Case.CourtId,
                                 CaseTypeLabel = a.Case.CaseType.Label,
                                 CaseRegNumber = a.Case.RegNumber,
                                 CaseYear = a.Case.RegDate.Year,
                                 CaseSessionId = a.CaseSessionId,
                                 CaseSessionActId = a.Id,
                                 ActDeclaredDate = a.ActDeclaredDate ?? DateTime.Now,
                                 ActRegNumber = a.RegNumber,
                                 ActTypeLabel = a.ActType.Label,
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataTakingActionCaseFastProcessN4(int caseSessionActId)
        {
            return await repo.AllReadonly<CaseSessionAct>()
                             .Where(a => a.Id == caseSessionActId)
                             .Where(a => a.CaseId != null)
                             .Where(a => a.Case.IsFastProcess ?? false)
                             .Where(a => a.ActDeclaredDate != null)
                             .Where(a => NomenclatureConstants.ActType.NotificationN4.Contains(a.ActTypeId))
                             .Select(a => new CaseInfoFastProcessVM()
                             {
                                 CaseId = a.CaseId ?? 0,
                                 CaseCourtId = a.Case.CourtId,
                                 CaseTypeLabel = a.Case.CaseType.Label,
                                 CaseRegNumber = a.Case.RegNumber,
                                 CaseYear = a.Case.RegDate.Year,
                                 CaseSessionId = a.CaseSessionId,
                                 ActDeclaredDate = a.ActDeclaredDate ?? DateTime.Now,
                                 ActRegNumber = a.RegNumber,
                                 ActTypeLabel = a.ActType.Label,
                             })
                             .FirstOrDefaultAsync(); ;
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <param name="isCaseCode_0602_1_2_0604_1_2">Флаг за шифри 0602_1, 0602_2, 0604_1 и 0604_2</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataActInforcedАnotherInstanceFastProcess(int caseSessionActId, bool isCaseCode_0602_1_2_0604_1_2 = false)
        {
            Expression<Func<CaseSessionAct, bool>> caseCodeWhere = a => !NomenclatureConstants.CaseCode.CaseCode_0602_1_2_0604_1_2.Contains(a.Case.CaseCodeId ?? 0);
            if (isCaseCode_0602_1_2_0604_1_2)
                caseCodeWhere = a => NomenclatureConstants.CaseCode.CaseCode_0602_1_2_0604_1_2.Contains(a.Case.CaseCodeId ?? 0);

            CaseSessionAct sessionAct = await repo.AllReadonly<CaseSessionAct>()
                                                  .Where(x => x.Id == caseSessionActId)
                                                  .Where(x => x.ActInforcedDate != null)
                                                  .Where(x => x.IsFinalDoc)
                                                  .Where(caseCodeWhere)
                                                  .FirstOrDefaultAsync();

            if (sessionAct is null)
                return null;

            List<int> priorCaseIds = await GetPriorCaseIds(sessionAct.CaseId ?? 0);
            priorCaseIds.Add(sessionAct.CaseId ?? 0);

            CaseInfoFastProcessVM result = await repo.AllReadonly<Case>()
                                                     .Where(c => priorCaseIds.Contains(c.Id))
                                                     .Where(c => c.CaseMigrations.Any(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                           (m.PriorCase.IsFastProcess ?? false) &&
                                                                                           m.DateExpired == null))
                                                     .Select(c => new CaseInfoFastProcessVM()
                                                     {
                                                         CaseId = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                              (m.PriorCase.IsFastProcess ?? false) &&
                                                                                              m.DateExpired == null)
                                                                                  .Select(m => m.PriorCaseId)
                                                                                  .FirstOrDefault(),
                                                         CaseCourtId = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                   (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                   m.DateExpired == null)
                                                                                       .Select(m => m.PriorCase.CourtId)
                                                                                       .FirstOrDefault(),
                                                         CaseTypeLabel = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                     (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                     m.DateExpired == null)
                                                                                         .Select(m => m.PriorCase.CaseType.Label)
                                                                                         .FirstOrDefault(),
                                                         CaseRegNumber = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                     (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                     m.DateExpired == null)
                                                                                         .Select(m => m.PriorCase.RegNumber)
                                                                                         .FirstOrDefault(),
                                                         CaseYear = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                m.DateExpired == null)
                                                                                    .Select(m => m.PriorCase.RegDate.Year)
                                                                                    .FirstOrDefault(),
                                                         CaseSessionId = null,
                                                     })
                                                     .FirstOrDefaultAsync();

            if (result is not null)
                result.ActInforcedDate = sessionAct.ActInforcedDate;

            return result;
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataActDeclaredAnotherInstanceFastProcess(int caseSessionActId)
        {
            CaseSessionAct sessionAct = await repo.AllReadonly<CaseSessionAct>()
                                                  .Where(x => x.Id == caseSessionActId)
                                                  .Where(x => x.ActDeclaredDate != null)
                                                  .Where(x => x.IsFinalDoc)
                                                  .FirstOrDefaultAsync();

            if (sessionAct is null)
                return null;

            List<int> priorCaseIds = await GetPriorCaseIds(sessionAct.CaseId ?? 0);
            priorCaseIds.Add(sessionAct.CaseId ?? 0);

            CaseInfoFastProcessVM result = await repo.AllReadonly<Case>()
                                                     .Where(c => priorCaseIds.Contains(c.Id))
                                                     .Where(c => c.CaseMigrations.Any(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                           (m.PriorCase.IsFastProcess ?? false) &&
                                                                                           m.DateExpired == null))
                                                     .Select(c => new CaseInfoFastProcessVM()
                                                     {
                                                         CaseId = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                              (m.PriorCase.IsFastProcess ?? false) &&
                                                                                              m.DateExpired == null)
                                                                                  .Select(m => m.PriorCaseId)
                                                                                  .FirstOrDefault(),
                                                         CaseCourtId = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                   (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                   m.DateExpired == null)
                                                                                       .Select(m => m.PriorCase.CourtId)
                                                                                       .FirstOrDefault(),
                                                         CaseTypeLabel = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                     (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                     m.DateExpired == null)
                                                                                         .Select(m => m.PriorCase.CaseType.Label)
                                                                                         .FirstOrDefault(),
                                                         CaseRegNumber = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                     (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                     m.DateExpired == null)
                                                                                         .Select(m => m.PriorCase.RegNumber)
                                                                                         .FirstOrDefault(),
                                                         CaseYear = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                m.DateExpired == null)
                                                                                    .Select(m => m.PriorCase.RegDate.Year)
                                                                                    .FirstOrDefault(),
                                                         CaseSessionId = null,
                                                     })
                                                     .FirstOrDefaultAsync();

            if (result is not null)
                result.ActDeclaredDate = sessionAct.ActDeclaredDate;

            return result;
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="isCaseCode_0604_1_2">Флаг за шифри 0604_1 и 0604_2</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataNewCaseHigherInstanceFastProcess(int caseId, bool isCaseCode_0604_1_2 = false)
        {
            Expression<Func<Case, bool>> caseCodeWhere = c => !NomenclatureConstants.CaseCode.CaseCode_0604_1_2.Contains(c.CaseCodeId ?? 0);
            if (isCaseCode_0604_1_2)
                caseCodeWhere = c => NomenclatureConstants.CaseCode.CaseCode_0604_1_2.Contains(c.CaseCodeId ?? 0);

            List<int> priorCaseIds = await GetPriorCaseIds(caseId);
            priorCaseIds.Add(caseId);

            DateTime regDate = await repo.AllReadonly<Case>()
                                         .Where(x => x.Id == caseId)
                                         .Select(x => x.RegDate)
                                         .FirstAsync();

            CaseInfoFastProcessVM result = await repo.AllReadonly<Case>()
                                                     .Where(c => priorCaseIds.Contains(c.Id))
                                                     .Where(caseCodeWhere)
                                                     .Where(c => c.CaseMigrations.Any(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                           (m.PriorCase.IsFastProcess ?? false) &&
                                                                                           m.DateExpired == null))
                                                     .Select(c => new CaseInfoFastProcessVM()
                                                     {
                                                         CaseId = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                              (m.PriorCase.IsFastProcess ?? false) &&
                                                                                              m.DateExpired == null)
                                                                                  .Select(m => m.PriorCaseId)
                                                                                  .FirstOrDefault(),
                                                         CaseCourtId = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                   (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                   m.DateExpired == null)
                                                                                       .Select(m => m.PriorCase.CourtId)
                                                                                       .FirstOrDefault(),
                                                         CaseTypeLabel = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                     (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                     m.DateExpired == null)
                                                                                         .Select(m => m.PriorCase.CaseType.Label)
                                                                                         .FirstOrDefault(),
                                                         CaseRegNumber = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                     (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                     m.DateExpired == null)
                                                                                         .Select(m => m.PriorCase.RegNumber)
                                                                                         .FirstOrDefault(),
                                                         CaseYear = c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                                                (m.PriorCase.IsFastProcess ?? false) &&
                                                                                                m.DateExpired == null)
                                                                                    .Select(m => m.PriorCase.RegDate.Year)
                                                                                    .FirstOrDefault(),
                                                         CaseSessionId = null,
                                                         CaseRegDate = c.RegDate
                                                     })
                                                     .FirstOrDefaultAsync();

            if (result is not null)
                result.CaseRegDate = regDate;

            return result;
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataCompliantDocumentCaseFastProcess(long documentId)
        {
            return await repo.AllReadonly<DocumentCaseInfo>()
                             .Where(d => d.DocumentId == documentId)
                             .Where(d => d.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                             .Where(d => d.CaseId != null)
                             .Where(d => d.Case.IsFastProcess ?? false)
                             .Select(d => new CaseInfoFastProcessVM()
                             {
                                 CaseId = d.CaseId ?? 0,
                                 CaseCourtId = d.Case.CourtId,
                                 CaseTypeLabel = d.Case.CaseType.Label,
                                 CaseRegNumber = d.Case.RegNumber,
                                 CaseYear = d.Case.RegDate.Year,
                                 DocumentDate = d.Document.DocumentDate,
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Проверка на документ дали е съпровождащ документ - други - становище
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        private async Task<int?> GetCaseIdForExpiredNotificationsForExpressingOpinionObjectionFastProcess(long documentId)
        {
            return await repo.AllReadonly<DocumentCaseInfo>()
                             .Where(d => d.DocumentId == documentId)
                             .Where(d => d.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                             .Where(d => d.Document.DocumentGroupId == DocumentConstants.DocumentGroupConstants.Other)
                             .Where(d => d.Document.DocumentTypeId == DocumentConstants.Types.Opinion)
                             .Where(d => d.CaseId != null)
                             .Where(d => d.Case.IsFastProcess ?? false)
                             .Select(d => d.CaseId)
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Проверка на документ дали е съпровождащ документ - Възражение
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        private async Task<int?> GetCaseIdForLackSubmittedObjectionFastProcess(long documentId)
        {
            return await repo.AllReadonly<DocumentCaseInfo>()
                             .Where(d => d.DocumentId == documentId)
                             .Where(d => d.Document.DocumentGroup.DocumentKindId == DocumentConstants.DocumentKind.CompliantDocument)
                             .Where(d => d.Document.DocumentGroupId == DocumentConstants.DocumentGroupConstants.Objection)
                             .Where(d => d.CaseId != null)
                             .Where(d => d.Case.IsFastProcess ?? false)
                             .Select(d => d.CaseId)
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataN24(long documentId)
        {
            return await repo.AllReadonly<DocumentCaseInfo>()
                             .Where(d => d.DocumentId == documentId)
                             .Where(d => NomenclatureConstants.DocumentGroup.N24.Contains(d.Document.DocumentGroupId))
                             .Where(d => d.CaseId != null)
                             .Where(d => d.Case.IsFastProcess ?? false)
                             .Select(d => new CaseInfoFastProcessVM()
                             {
                                 CaseId = d.CaseId ?? 0,
                                 CaseCourtId = d.Case.CourtId,
                                 CaseTypeLabel = d.Case.CaseType.Label,
                                 CaseRegNumber = d.Case.RegNumber,
                                 CaseYear = d.Case.RegDate.Year,
                                 DocumentDate = d.Document.DocumentDate,
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Метод връщащ основно кюери за призовки
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на призовки</param>
        /// <returns></returns>
        private IQueryable<CaseNotification> GetQueryCaseNotification(int caseNotificationId)
        {
            return repo.AllReadonly<CaseNotification>()
                       .Where(n => n.Id == caseNotificationId)
                       .Where(n => n.Case.IsFastProcess ?? false);
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataNoProceduralActionTakenFastProcess(int caseNotificationId)
        {
            return await GetQueryCaseNotification(caseNotificationId).Where(n => n.CaseSession.CaseSessionActs.Any(a => a.DateExpired == null &&
                                                                                                                        a.ActDeclaredDate != null &&
                                                                                                                        (a.NotificationOn ?? false) &&
                                                                                                                        ((a.NotificationDays ?? 0) > 0 ||
                                                                                                                         (a.NotificationWeeks ?? 0) > 0 ||
                                                                                                                         (a.NotificationMonts ?? 0) > 0)))
                                                                     .Select(n => new CaseInfoFastProcessVM()
                                                                     {
                                                                         CaseId = n.CaseId,
                                                                         CaseSessionId = n.CaseSessionId,
                                                                         CaseCourtId = n.Case.CourtId,
                                                                         CaseTypeLabel = n.Case.CaseType.Label,
                                                                         CaseRegNumber = n.Case.RegNumber,
                                                                         CaseYear = n.Case.RegDate.Year,
                                                                         NotificationDeliveryPersonFullName = n.CasePerson.FullName,
                                                                         CaseSessionActId = n.CaseSession.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                                                                                                     a.ActDeclaredDate != null &&
                                                                                                                                     (a.NotificationOn ?? false))
                                                                                                                         .Select(a => (int?)a.Id)
                                                                                                                         .FirstOrDefault(),
                                                                         DaysFastProcess = n.CaseSession.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                                                                                                     a.ActDeclaredDate != null &&
                                                                                                                                     (a.NotificationOn ?? false))
                                                                                                                         .Select(a => a.NotificationDays)
                                                                                                                         .FirstOrDefault(),
                                                                         WeeksFastProcess = n.CaseSession.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                                                                                                     a.ActDeclaredDate != null &&
                                                                                                                                     (a.NotificationOn ?? false))
                                                                                                                         .Select(a => a.NotificationWeeks)
                                                                                                                         .FirstOrDefault(),
                                                                         MontsFastProcess = n.CaseSession.CaseSessionActs.Where(a => a.DateExpired == null &&
                                                                                                                                     a.ActDeclaredDate != null &&
                                                                                                                                     (a.NotificationOn ?? false))
                                                                                                                         .Select(a => a.NotificationMonts)
                                                                                                                         .FirstOrDefault(),
                                                                     })
                                                                     .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        private async Task<List<CaseInfoFastProcessVM>> GetDataFromActNoProceduralActionTakenFastProcess(int caseSessionActId)
        {
            return await repo.AllReadonly<CaseSessionAct>()
                             .Where(a => a.Case.IsFastProcess ?? false)
                             .Where(a => a.ActDeclaredDate != null)
                             .Where(a => a.DateExpired == null)
                             .Where(a => a.Id == caseSessionActId)
                             .SelectMany(a => a.CaseSession
                                               .CaseNotifications
                                               .Where(n => n.DateExpired == null &&
                                                           n.NotificationStateId == NomenclatureConstants.NotificationState.Delivered &&
                                                           n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier &&
                                                           n.DeliveryItems.Any(d => d.DateExpired == null &&
                                                                                    d.NotificationStateId == NomenclatureConstants.NotificationState.Delivered))
                                               .Select(n => new CaseInfoFastProcessVM()
                                               {
                                                   CaseId = n.CaseId,
                                                   CaseSessionId = n.CaseSessionId,
                                                   CaseCourtId = n.Case.CourtId,
                                                   CaseTypeLabel = n.Case.CaseType.Label,
                                                   CaseRegNumber = n.Case.RegNumber,
                                                   CaseYear = n.Case.RegDate.Year,
                                                   NotificationDeliveryPersonFullName = n.CasePerson.FullName,
                                                   CaseNotificationId = n.Id,
                                                   CaseSessionActId = a.Id,
                                                   CaseSessionActNotificationOn = a.NotificationOn ?? false,
                                                   DaysFastProcess = a.NotificationDays,
                                                   WeeksFastProcess = a.NotificationWeeks,
                                                   MontsFastProcess = a.NotificationMonts,
                                                   EventDate = n.DeliveryItems.Where(d => d.DateExpired == null &&
                                                                                          d.NotificationStateId == NomenclatureConstants.NotificationState.Delivered)
                                                                              .Select(d => d.DeliveryDate)
                                                                              .FirstOrDefault()
                                               })
                                               .ToList())
                             .ToListAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="isNotEpep">Флаг, дали да изключва призовките от ЕПЕП</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataMessageDeliveredFastProcess(int caseNotificationId, bool isNotEpep = false)
        {
            Expression<Func<CaseNotification, bool>> isNotEpepWhere = n => true;
            if (isNotEpep)
                isNotEpepWhere = n => n.NotificationDeliveryGroupId != NomenclatureConstants.NotificationDeliveryGroup.ByEPEP;

            return await GetQueryCaseNotification(caseNotificationId).Where(n => n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier)
                                                                     .Where(isNotEpepWhere)
                                                                     .Select(n => new CaseInfoFastProcessVM()
                                                                     {
                                                                         CaseId = n.CaseId,
                                                                         CaseSessionId = n.CaseSessionId,
                                                                         CaseCourtId = n.Case.CourtId,
                                                                         CaseTypeLabel = n.Case.CaseType.Label,
                                                                         CaseRegNumber = n.Case.RegNumber,
                                                                         CaseYear = n.Case.RegDate.Year,
                                                                         NotificationDeliveryPersonFullName = n.CasePerson.FullName,
                                                                         NotificationToCourtLabel = n.ToCourtId != null ? n.ToCourt.Label : string.Empty,
                                                                         NotificationLawUnitId = n.LawUnitId
                                                                     })
                                                                     .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataMessageDeliveredFastProcessWithSummons(int caseNotificationId)
        {
            return await GetQueryCaseNotification(caseNotificationId).Where(n => n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier &&
                                                                                 n.NotificationDeliveryGroupId == NomenclatureConstants.NotificationDeliveryGroup.WithSummons)
                                                                     .Select(n => new CaseInfoFastProcessVM()
                                                                     {
                                                                         CaseId = n.CaseId,
                                                                         CaseSessionId = n.CaseSessionId,
                                                                         CaseCourtId = n.Case.CourtId,
                                                                         CaseTypeLabel = n.Case.CaseType.Label,
                                                                         CaseRegNumber = n.Case.RegNumber,
                                                                         CaseYear = n.Case.RegDate.Year,
                                                                         NotificationDeliveryPersonFullName = n.CasePerson.FullName,
                                                                         NotificationToCourtLabel = n.ToCourtId != null ? n.ToCourt.Label : string.Empty,
                                                                         NotificationLawUnitId = n.LawUnitId
                                                                     })
                                                                     .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataLackSubmittedObjectionFastProcess(int caseNotificationId)
        {
            return await GetQueryCaseNotification(caseNotificationId).Where(n => n.CaseSessionActId != null)
                                                                     .Where(n => n.CaseSessionAct.ActTypeId == NomenclatureConstants.ActType.CommandmentForExec)
                                                                     .Where(n => n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier)
                                                                     .Select(n => new CaseInfoFastProcessVM()
                                                                     {
                                                                         CaseId = n.CaseId,
                                                                         CaseSessionId = n.CaseSessionId,
                                                                         CaseCourtId = n.Case.CourtId,
                                                                         CaseTypeLabel = n.Case.CaseType.Label,
                                                                         CaseRegNumber = n.Case.RegNumber,
                                                                         CaseYear = n.Case.RegDate.Year,
                                                                         NotificationDeliveryPersonFullName = n.CasePerson.FullName
                                                                     })
                                                                     .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataReceivedMessageDeliveryFastProcess(int caseNotificationId)
        {
            return await GetQueryCaseNotification(caseNotificationId).Where(n => n.CourtId != n.ToCourtId)
                                                                     .Select(n => new CaseInfoFastProcessVM()
                                                                     {
                                                                         CaseId = n.CaseId,
                                                                         CaseSessionId = n.CaseSessionId,
                                                                         CaseCourtId = n.Case.CourtId,
                                                                         CaseTypeLabel = n.Case.CaseType.Label,
                                                                         CaseRegNumber = n.Case.RegNumber,
                                                                         CaseYear = n.Case.RegDate.Year,
                                                                         NotificationDeliveryPersonFullName = n.CasePerson.FullName
                                                                     })
                                                                     .FirstOrDefaultAsync();
        }


        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataLackExpressingOpinionObjectionFastProcess(int caseNotificationId)
        {
            return await GetQueryCaseNotification(caseNotificationId).Where(n => n.HtmlTemplateId == NomenclatureConstants.HtmlTemplateConstants.NotificationArt414A &&
                                                                                 n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier)
                                                                     .Select(n => new CaseInfoFastProcessVM()
                                                                     {
                                                                         CaseId = n.CaseId,
                                                                         CaseSessionId = n.CaseSessionId,
                                                                         CaseCourtId = n.Case.CourtId,
                                                                         CaseTypeLabel = n.Case.CaseType.Label,
                                                                         CaseRegNumber = n.Case.RegNumber,
                                                                         CaseYear = n.Case.RegDate.Year,
                                                                         NotificationDeliveryPersonFullName = n.CasePerson.FullName,
                                                                     })
                                                                     .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataFilingClaimFastProcess(int caseNotificationId)
        {
            return await GetQueryCaseNotification(caseNotificationId).Where(n => NomenclatureConstants.HtmlTemplateConstants.FilingClaimFastProcessArray.Contains(n.HtmlTemplateId ?? 0) &&
                                                                                 n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier)
                                                                     .Select(n => new CaseInfoFastProcessVM()
                                                                     {
                                                                         CaseId = n.CaseId,
                                                                         CaseSessionId = n.CaseSessionId,
                                                                         CaseCourtId = n.Case.CourtId,
                                                                         CaseTypeLabel = n.Case.CaseType.Label,
                                                                         CaseRegNumber = n.Case.RegNumber,
                                                                         CaseYear = n.Case.RegDate.Year,
                                                                         NotificationDeliveryPersonFullName = n.CasePerson.FullName,
                                                                     })
                                                                     .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за създаване на всички нотификации за потребител, който се добавя в по-късен етап
        /// </summary>
        /// <param name="caseLawUnitId"></param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataCreateNotificationsAddCaseLawUnit(int caseLawUnitId)
        {
            return await repo.AllReadonly<CaseLawUnit>()
                             .Where(x => x.Id == caseLawUnitId)
                             .Select(x => new CaseInfoFastProcessVM()
                             {
                                 CaseId = x.CaseId,
                                 CaseCourtId = x.Case.CourtId,
                                 CaseTypeLabel = x.Case.CaseType.Label,
                                 CaseRegNumber = x.Case.RegNumber,
                                 CaseYear = x.Case.RegDate.Year,
                                 CaseLawUnitSmall = new()
                                 {
                                     LawUnitId = x.LawUnitId,
                                     LawUnitUserId = x.LawUnitUserId,
                                     LawUnitName = x.LawUnit.FullName,
                                     JudgeRoleId = x.JudgeRoleId,
                                     DepartmentLabel = (x.CourtDepartmentId != null) ? " " + x.CourtDepartment.Label : string.Empty,
                                     DepartmentId = x.CourtDepartmentId,
                                 }
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        private async Task<int?> GetValidateCaseIdExpiredNotificationsForFilingClaimFastProcess(int caseId)
        {
            Expression<Func<Case, bool>> caseCodeWhere = c => NomenclatureConstants.CaseCode.CaseCode_0602_1_2_0604_1_2.Contains(c.CaseCodeId ?? 0);

            return await repo.AllReadonly<Case>()
                             .Where(c => c.Id == caseId)
                             .Where(caseCodeWhere)
                             .Where(c => c.CaseMigrations.Any(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                   (m.PriorCase.IsFastProcess ?? false) &&
                                                                   m.DateExpired == null))
                             .Select(c => c.CaseMigrations.Where(m => NomenclatureConstants.CaseMigrationTypes.NewCaseHigherInstanceFastProcess.Contains(m.CaseMigrationTypeId) &&
                                                                      (m.PriorCase.IsFastProcess ?? false) &&
                                                                      m.DateExpired == null)
                                                          .Select(m => (int?)m.PriorCaseId)
                                                          .FirstOrDefault())

                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="actId">Идентификатор на акт</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataDecreeRecusalSelfRecusalFastProcessByActId(int actId)
        {
            return await repo.AllReadonly<CaseSessionAct>()
                             .Where(a => a.Id == actId)
                             .Where(a => a.ActDeclaredDate != null)
                             .Where(a => a.CaseSession
                                          .CaseSessionResults
                                          .Any(r => NomenclatureConstants.CaseSessionResult
                                                                         .DecreeRecusalSelfRecusalFastProcessArray
                                                                         .Contains(r.SessionResultId)))
                             .Where(d => d.CaseId != null)
                             .Where(d => d.Case.IsFastProcess ?? false)
                             .Select(d => new CaseInfoFastProcessVM()
                             {
                                 CaseId = d.CaseId ?? 0,
                                 CaseCourtId = d.Case.CourtId,
                                 CaseTypeLabel = d.Case.CaseType.Label,
                                 CaseRegNumber = d.Case.RegNumber,
                                 CaseYear = d.Case.RegDate.Year,
                                 ActDeclaredDate = d.ActDeclaredDate
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="sessionResultId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataDecreeRecusalSelfRecusalFastProcessBySessionResultId(int sessionResultId)
        {
            return await repo.AllReadonly<Infrastructure.Data.Models.Cases.CaseSessionResult>()
                             .Where(r => r.Id == sessionResultId)
                             .Where(r => NomenclatureConstants.CaseSessionResult
                                                              .DecreeRecusalSelfRecusalFastProcessArray
                                                              .Contains(r.SessionResultId))
                             .Where(r => r.CaseSession.CaseSessionActs.Any(a => a.ActDeclaredDate != null))
                             .Where(d => d.CaseId != null)
                             .Where(d => d.Case.IsFastProcess ?? false)
                             .Select(d => new CaseInfoFastProcessVM()
                             {
                                 CaseId = d.CaseId ?? 0,
                                 CaseCourtId = d.Case.CourtId,
                                 CaseTypeLabel = d.Case.CaseType.Label,
                                 CaseRegNumber = d.Case.RegNumber,
                                 CaseYear = d.Case.RegDate.Year,
                                 ActDeclaredDate = d.CaseSession.CaseSessionActs.Where(a => a.ActDeclaredDate != null).Select(a => a.ActDeclaredDate).FirstOrDefault(),
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело през акт
        /// </summary>
        /// <param name="sessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataDeliveredЕxecutiveListFastProcessByAct(int sessionActId)
        {
            return await repo.AllReadonly<CaseSessionAct>()
                             .Where(x => x.Id == sessionActId)
                             .Select(d => new CaseInfoFastProcessVM()
                             {
                                 CaseId = d.CaseId ?? 0,
                                 CaseCourtId = d.Case.CourtId,
                                 CaseTypeLabel = d.Case.CaseType.Label,
                                 CaseRegNumber = d.Case.RegNumber,
                                 CaseYear = d.Case.RegDate.Year,
                                 ExecListNumber = d.RegNumber
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело през акт
        /// </summary>
        /// <param name="execListId">Идентификатор на изпълнителен лист</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataDeliveredЕxecutiveListFastProcessByExecList(int execListId)
        {
            return await repo.AllReadonly<ExecList>()
                             .Where(x => x.Id == execListId)
                             .Select(x => new CaseInfoFastProcessVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 CaseCourtId = x.Case.CourtId,
                                 CaseTypeLabel = x.Case.CaseType.Label,
                                 CaseRegNumber = x.Case.RegNumber,
                                 CaseYear = x.Case.RegDate.Year,
                                 ExecListNumber = x.RegNumber
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="sourceType">Тип на обекта</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataDeliveredЕxecutiveListFastProcess(int sourceType, long sourceId)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct: return await GetDataDeliveredЕxecutiveListFastProcessByAct((int)sourceId);
                case SourceTypeSelectVM.ExecList: return await GetDataDeliveredЕxecutiveListFastProcessByExecList((int)sourceId);
                default: return null;
            }
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        private async Task<CaseInfoFastProcessVM> GetDataN23(int caseSessionActId)
        {
            return await repo.AllReadonly<CaseSessionAct>()
                             .Where(x => x.Id == caseSessionActId)
                             .Where(x => x.ActDeclaredDate != null)
                             .Where(x => x.Case.IsFastProcess ?? false)
                             .Where(x => x.PeriodNotifications.Any(p => p.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N23))
                             .Select(x => new CaseInfoFastProcessVM()
                             {
                                 CaseId = x.CaseId ?? 0,
                                 CaseCourtId = x.Case.CourtId,
                                 CaseTypeLabel = x.Case.CaseType.Label,
                                 CaseRegNumber = x.Case.RegNumber,
                                 CaseYear = x.Case.RegDate.Year,
                                 ActDeclaredDate = x.ActDeclaredDate,
                                 CaseSessionActNotificationOn = x.PeriodNotifications.Where(p => p.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N23).Select(p => p.NotificationOn ?? false).FirstOrDefault(),
                                 DaysFastProcess = x.PeriodNotifications.Where(p => p.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N23).Select(p => p.NotificationDays).FirstOrDefault(),
                                 WeeksFastProcess = x.PeriodNotifications.Where(p => p.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N23).Select(p => p.NotificationWeeks).FirstOrDefault(),
                                 MontsFastProcess = x.PeriodNotifications.Where(p => p.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N23).Select(p => p.NotificationMonts).FirstOrDefault(),
                                 Description = x.PeriodNotifications.Where(p => p.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N23).Select(p => p.Description).FirstOrDefault()
                             })
                             .FirstOrDefaultAsync();
        }

        #endregion

        #region Общи методи

        /// <summary>
        /// Извличане на кюери за съкратени данни за лица в дело
        /// </summary>
        /// <param name="typePersonGet">Кои лица искаме да извлечем: 1 - Без ръчни роли на служители / 2 - само ръчни роли / 3 - съдия докладчик / всички</param>
        /// <param name="dateTime">Дата към която да се гледат лицата</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private IQueryable<CaseLawUnitSmallVM> GetQueryCaseLawUnit(int typePersonGet, DateTime dateTime, int caseId, int? caseSessionId)
        {
            Expression<Func<CaseLawUnit, bool>> dateFromWhere = x => x.DateFrom <= dateTime; ;
            Expression<Func<CaseLawUnit, bool>> dateToWhere = x => (x.DateTo ?? dateTime) >= dateTime;


            Expression<Func<CaseLawUnit, bool>> typePersonGetWhere = x => true;
            switch (typePersonGet)
            {
                case 1:
                    typePersonGetWhere = x => !NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId);
                    break;
                case 2:
                    typePersonGetWhere = x => NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId);
                    break;
                case 3:
                    typePersonGetWhere = x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter;
                    break;
            }

            Expression<Func<CaseLawUnit, bool>> caseSessionIdWhere = x => x.CaseSessionId == null;
            if ((caseSessionId ?? 0) > 0)
                caseSessionIdWhere = x => x.CaseSessionId == caseSessionId;

            return repo.AllReadonly<CaseLawUnit>()
                       .Where(x => x.CaseId == caseId)
                       .Where(dateFromWhere)
                       .Where(dateToWhere)
                       .Where(typePersonGetWhere)
                       .Where(caseSessionIdWhere)
                       .Select(x => new CaseLawUnitSmallVM()
                       {
                           Id = x.Id,
                           LawUnitId = x.LawUnitId,
                           LawUnitUserId = x.LawUnitUserId,
                           LawUnitName = x.LawUnit.FullName,
                           JudgeRoleId = x.JudgeRoleId,
                           DepartmentLabel = (x.CourtDepartmentId != null) ? " " + x.CourtDepartment.Label : string.Empty,
                           DepartmentId = x.CourtDepartmentId,
                       })
                       .AsQueryable();
        }

        /// <summary>
        /// Извличане на съкратени данни за служители
        /// </summary>
        /// <param name="typePersonGet">Кои лица искаме да извлечем: 1 - Без ръчни роли на служители / 2 - само ръчни роли / всички</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<List<CaseLawUnitSmallVM>> CaseLawUnitSmallSelectAsync(int typePersonGet, int caseId, int? caseSessionId)
        {
            DateTime dateTime = ((caseSessionId ?? 0) == 0) ? DateTime.Now : await GetPropByIdAsync<CaseSession, DateTime>(caseSessionId.Value, x => x.DateFrom);
            return await GetQueryCaseLawUnit(typePersonGet, dateTime, caseId, caseSessionId).ToListAsync();
        }

        /// <summary>
        /// Извличане на съкратени данни за служители
        /// </summary>
        /// <param name="typePersonGet">Кои лица искаме да извлечем: 1 - Без ръчни роли на служители / 2 - само ръчни роли / всички</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private List<CaseLawUnitSmallVM> CaseLawUnitSmallSelect(int typePersonGet, int caseId, int? caseSessionId)
        {
            DateTime dateTime = ((caseSessionId ?? 0) == 0) ? DateTime.Now : GetPropById<CaseSession, DateTime>(caseSessionId.Value, x => x.DateFrom);
            return GetQueryCaseLawUnit(typePersonGet, dateTime, caseId, caseSessionId).ToList();
        }

        /// <summary>
        /// Метод връщащ заместващите на съдия
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="lawUnitId">Идентификатор на лице</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        private async Task<List<CaseLawUnitSmallVM>> GetLawUnitSubstitution(int courtId, int lawUnitId, DateTime dateTimeEvent)
        {
            return await repo.AllReadonly<CourtLawUnitSubstitution>()
                             .Where(x => x.CourtId == courtId)
                             .Where(x => x.LawUnitId == lawUnitId)
                             .Where(x => x.DateFrom <= dateTimeEvent)
                             .Where(x => x.DateTo >= dateTimeEvent)
                             .Where(x => x.DateExpired == null)
                             .Select(x => new CaseLawUnitSmallVM()
                             {
                                 LawUnitUserId = x.SubstituteLawUnit.ApplicationUsers.Where(u => u.IsActive).Select(u => u.Id).FirstOrDefault(),
                                 LawUnitName = x.SubstituteLawUnit.FullName,
                                 IsLawUnitSubstitution = true
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Метод връщащ състава по дело/заседание с извличане от конфигурация за тип нотификация, за кой от делото се отнася
        /// </summary>
        /// <param name="workNotificationTypeId">Идентификатор на нотификация</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="withSubstitution">Флаг дали да изчете заместващите на съдия-докладчика</param>
        /// <returns></returns>
        private async Task<List<CaseLawUnitSmallVM>> GetCaseLawUnitFastProcess(int workNotificationTypeId, int courtId, int caseId, int? caseSessionId, DateTime dateTimeEvent, bool withSubstitution = true)
        {
            int typePersonGet = await repo.AllReadonly<Infrastructure.Data.Models.Nomenclatures.WorkNotificationType>()
                                          .Where(x => x.Id == workNotificationTypeId)
                                          .Select(x => x.TypePersonLoad)
                                          .FirstOrDefaultAsync() ?? 0;

            return await GetCaseLawUnitAllFastProcess(typePersonGet, courtId, caseId, caseSessionId, dateTimeEvent, withSubstitution);
        }

        /// <summary>
        /// Метод връщащ състава по дело/заседание
        /// </summary>
        /// <param name="typePersonGet">Кои лица искаме да извлечем: 1 - Без ръчни роли на служители / 2 - само ръчни роли / 3 - съдия докладчик / всички</param>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="withSubstitution">Флаг дали да изчете заместващите на съдия-докладчика</param>
        /// <returns></returns>
        private async Task<List<CaseLawUnitSmallVM>> GetCaseLawUnitAllFastProcess(int typePersonGet, int courtId, int caseId, int? caseSessionId, DateTime dateTimeEvent, bool withSubstitution = true)
        {
            List<CaseLawUnitSmallVM> lawUnits = [];

            if (typePersonGet == 3)
                lawUnits.Add((await CaseLawUnitSmallSelectAsync(typePersonGet, caseId, caseSessionId)).FirstOrDefault());
            else
                lawUnits = await CaseLawUnitSmallSelectAsync(typePersonGet, caseId, caseSessionId);

            if (withSubstitution)
            {
                CaseLawUnitSmallVM judgeReporter = lawUnits.Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
                if (judgeReporter != null)
                    lawUnits.AddRange(await GetLawUnitSubstitution(courtId, judgeReporter.LawUnitId, dateTimeEvent));
            }

            return lawUnits;
        }

        /// <summary>
        /// Метод връщащ състава по дело/заседание - само ръчните роли
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<List<CaseLawUnitSmallVM>> GetCaseLawUnitManualRolesFastProcess(int caseId, int? caseSessionId)
        {
            return await GetCaseLawUnitAllFastProcess(2, 0, caseId, caseSessionId, DateTime.Now, false);
        }

        /// <summary>
        /// Метод връщащ само съдия докладчик по дело/заседание
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private CaseLawUnitSmallVM GetOnlyJudgeReporterFastProcess(int caseId, int? caseSessionId)
        {
            return CaseLawUnitSmallSelect(0, caseId, caseSessionId).Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
        }

        /// <summary>
        /// Метод връщащ само съдия докладчик по дело/заседание
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<CaseLawUnitSmallVM> GetOnlyJudgeReporterFastProcessAsync(int caseId, int? caseSessionId)
        {
            return (await GetCaseLawUnitAllFastProcess(0, 0, caseId, caseSessionId, DateTime.Now, false)).Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
        }

        /// <summary>
        /// Метод връщащ съдия докладчик по дело/заседание
        /// </summary>
        /// <param name="courtId">Идентификатор на съд</param>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        private async Task<List<CaseLawUnitSmallVM>> GetJudgeReporterFastProcess(int courtId, int caseId, int? caseSessionId, DateTime dateTimeEvent)
        {
            List<CaseLawUnitSmallVM> result = [];
            CaseLawUnitSmallVM judgeReporter = (await GetCaseLawUnitAllFastProcess(0, courtId, caseId, caseSessionId, dateTimeEvent, false)).Where(x => x.JudgeRoleId == NomenclatureConstants.JudgeRole.JudgeReporter).FirstOrDefault();
            result.Add(judgeReporter);
            result.AddRange(await GetLawUnitSubstitution(courtId, judgeReporter.LawUnitId, dateTimeEvent));
            return result;
        }

        /// <summary>
        /// Проверка дали съществува WorkNotification
        /// </summary>
        /// <param name="sourceType">SourceType</param>
        /// <param name="sourceId">SourceId</param>
        /// <param name="workNotificationTypeId">Тип нотификация</param>
        /// <param name="dateCreate">Дата на нотификация</param>
        /// <returns></returns>
        private async Task<bool> IsExistsNotificationsCaseFastProcess(int sourceType, long sourceId, int workNotificationTypeId, DateTime? dateCreate = null)
        {
            Expression<Func<WorkNotification, bool>> dateCreateWhere = x => true;
            if (dateCreate is not null)
                dateCreateWhere = x => x.DateCreated == dateCreate;

            return await repo.AllReadonly<WorkNotification>()
                             .Where(dateCreateWhere)
                             .AnyAsync(n => n.SourceType == sourceType &&
                                            n.SourceId == sourceId &&
                                            n.WorkNotificationTypeId == workNotificationTypeId &&
                                            n.DateExpired == null);
        }

        /// <summary>
        /// Валидация при създаване на нотификация за бързо производство
        /// </summary>
        /// <param name="caseData">Извлечени данни за дело</param>
        /// <param name="sourceId">SourceId</param>
        /// <param name="sourceType">SourceType</param>
        /// <param name="workNotificationTypeId">Идентификатор на тип на нотификацията</param>
        /// <param name="dateCreate">Дата на създаване на нотификацията</param>
        /// <returns></returns>
        private async Task<bool> ValidateForStartNotificationFastProcess(CaseInfoFastProcessVM caseData, long sourceId, int sourceType, int workNotificationTypeId, DateTime? dateCreate = null)
        {
            if (caseData == null)
                return false;

            if (await IsExistsNotificationsCaseFastProcess(sourceType, sourceId, workNotificationTypeId, dateCreate))
                return false;

            return true;
        }

        /// <summary>
        /// Запис на нотификация
        /// </summary>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="workNotificationTypeId">Тип на нотификацията</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        private async Task<string> SaveNotificationsForFastProcess(long sourceId, int workNotificationTypeId, bool saveChanges = true, DateTime? dateTimeEvent = null)
        {
            try
            {
                List<WorkNotification> notifications = [];

                switch (workNotificationTypeId)
                {
                    case NomenclatureConstants.WorkNotificationType.N1:
                        notifications = await GetNotificationsForN1((int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.N3:
                        notifications = await GetNotificationsForN3((int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.N3_From_Result:
                        notifications = await GetNotificationsForN3(null, (int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.CompliantDocumentCaseFastProcess:
                        notifications = await GetNotificationsForCompliantDocumentCaseFastProcess(sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.MessageDeliveredFastProcess:
                        notifications = await GetNotificationsForMessageDeliveredCaseFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.StuckMessagesFastProcess:
                        notifications = await GetNotificationsForStuckMessagesFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess:
                        notifications = await GetNotificationsForLackSubmittedObjectionFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.AppealActFastProcess:
                        notifications = await GetNotificationsForAppealActFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.ActInforcedAnotherInstanceFastProcess:
                        notifications = await GetNotificationsForActInforcedAnotherInstanceFastProcess((int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.N11:
                        notifications = await GetNotificationsForN11((int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWithout0604_1_2FastProcess:
                        notifications = await GetNotificationsForNewCaseHigherInstanceWithout0604_1_2FastProcess((int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWith0604_1_2FastProcess:
                        notifications = await GetNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess((int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.ExpressingOpinionObjectionFastProcess:
                        notifications = await GetNotificationsForExpressingOpinionObjectionFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.FilingClaimFastProcess:
                        notifications = await GetNotificationsForFilingClaimFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.ReceivedMessageDeliveryFastProcess:
                        notifications = await GetNotificationsForReceivedMessageDeliveryFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.NotDeliveredMessageDeliveryFastProcess:
                        notifications = await GetNotDeliveredMessageDeliveryFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.NoticeServiceReceivedFastProcess:
                        notifications = await GetNotificationsForNoticeServiceReceivedFastProcess((int)sourceId, dateTimeEvent ?? DateTime.Now);
                        break;
                    case NomenclatureConstants.WorkNotificationType.ActionTakenCourtOfficerDeclatActFastProcess:
                        notifications = await GetNotificationsForActionTakenCourtOfficerDeclatActFastProcess((int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.N23:
                        notifications = await GetNotificationsForN23((int)sourceId);
                        break;
                    case NomenclatureConstants.WorkNotificationType.N24:
                        notifications = await GetNotificationsForN24((int)sourceId);
                        break;
                }

                if (notifications != null && notifications.Count() > 0)
                {
                    repo.AddRange(notifications);
                    if (saveChanges)
                        await repo.SaveChangesAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на нотификация на sourceId:{sourceId} WorkNotificationTypeId: {workNotificationTypeId}");
                return ex.Message;
            }
        }

        /// <summary>
        /// Сторно на нотификация
        /// </summary>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="sourceType">Тип на обекта</param>
        /// <param name="workNotificationTypeId">Тип на нотификацията</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        private async Task<bool> ExpiredNotificationsForFastProcess(long sourceId, int sourceType, int workNotificationTypeId, bool saveChanges = true)
        {
            try
            {
                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(n => n.SourceId == sourceId &&
                                                                             n.SourceType == sourceType &&
                                                                             n.WorkNotificationTypeId == workNotificationTypeId &&
                                                                             n.DateExpired == null)
                                                                 .ToListAsync();

                notifications.ForEach(n => { n.DateExpired = DateTime.Now; n.UserExpiredId = userContext.UserId; });
                
                if (saveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сторно на нотификация SourceId:{sourceId} SourceType:{sourceType} WorkNotificationTypeId:{workNotificationTypeId}");
                return false;
            }
        }

        /// <summary>
        /// Сторно на нотификация
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="workNotificationTypeId">Тип на нотификацията</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        private async Task<bool> ExpiredNotificationsForFastProcess(int caseId, int workNotificationTypeId, bool saveChanges = true)
        {
            try
            {
                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(n => n.CaseId == caseId &&
                                                                             n.WorkNotificationTypeId == workNotificationTypeId &&
                                                                             n.DateExpired == null)
                                                                 .ToListAsync();

                if (notifications == null || notifications.Count < 1)
                    return true;

                notifications.ForEach(n => { n.DateExpired = DateTime.Now; n.UserExpiredId = userContext.UserId; });
                if (saveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при сторно на нотификация CaseId:{caseId} WorkNotificationTypeId:{workNotificationTypeId}");
                return false;
            }
        }

        /// <summary>
        /// Гасене на нотификация
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="workNotificationTypeId">Тип на нотификацията</param>
        /// <param name="descriptions">Причина за гасене на нотификация</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        private async Task<bool> TurnOfNotificationsForFastProcess(int caseId, int workNotificationTypeId, string descriptions, bool saveChanges = true)
        {
            try
            {
                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(n => n.CaseId == caseId &&
                                                                             n.WorkNotificationTypeId == workNotificationTypeId &&
                                                                             n.DateExpired == null &&
                                                                             n.DateTurnOff == null)
                                                                 .ToListAsync();

                if (notifications == null || notifications.Count < 1)
                    return true;

                notifications.ForEach(n => { n.DateTurnOff = DateTime.Now; n.DescriptionTurnOff = descriptions; });
                if (saveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотификация CaseId:{caseId} WorkNotificationTypeId:{workNotificationTypeId}");
                return false;
            }
        }

        /// <summary>
        /// Гасене на нотификация
        /// </summary>
        /// <param name="sourceType">Тип обект</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="workNotificationTypeId">Тип на нотификацията</param>
        /// <param name="descriptions">Причина за гасене на нотификация</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        private async Task<bool> TurnOfNotificationsForFastProcess(int sourceType, long sourceId, int workNotificationTypeId, string descriptions, bool saveChanges = true)
        {
            try
            {
                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(n => n.SourceType == sourceType &&
                                                                             n.SourceId == sourceId &&
                                                                             n.WorkNotificationTypeId == workNotificationTypeId &&
                                                                             n.DateExpired == null &&
                                                                             n.DateTurnOff == null)
                                                                 .ToListAsync();

                if (notifications == null || notifications.Count < 1)
                    return true;

                notifications.ForEach(n => { n.DateTurnOff = DateTime.Now; n.DescriptionTurnOff = descriptions; });
                if (saveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотификация sourceType:{sourceType} sourceId: {sourceId} WorkNotificationTypeId:{workNotificationTypeId}");
                return false;
            }
        }

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за връчено съобщение
        /// </summary>
        /// <param name="sourceType">Идентификатор на тип на обекта</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="workNotificationTypeId">Идентификатор на тип нотификация</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        private async Task<bool> EditDateEventNotificationsForFastProcess(int sourceType, long sourceId, int workNotificationTypeId, DateTime dateTimeEvent, bool saveChanges = true)
        {
            try
            {
                List<WorkNotification> workNotifications = await repo.All<WorkNotification>()
                                                                     .Where(n => n.SourceType == sourceType)
                                                                     .Where(n => n.SourceId == sourceId)
                                                                     .Where(n => n.WorkNotificationTypeId == workNotificationTypeId)
                                                                     .Where(n => n.DateExpired == null)
                                                                     .Where(n => n.DateTurnOff == null)
                                                                     .Where(n => n.DateCreated.Date != dateTimeEvent.Date)
                                                                     .ToListAsync();

                if (workNotifications == null || workNotifications.Count < 1)
                    return false;

                workNotifications.ForEach(n => { n.DateCreated = dateTimeEvent; n.DateRead = null; });

                if (saveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редакция на дата на нотификация тип на обект: {sourceType}, идентификатор на обект: {sourceId}, идентификатор на нотификация: {workNotificationTypeId}");
                return false;
            }
        }


        /// <summary>
        /// Метод връщащ начална дата и да не е в почивен ден
        /// </summary>
        /// <param name="eventDate">Дата на събитие</param>
        /// <returns></returns>
        private DateTime GetBeginDate(DateTime eventDate)
        {
            eventDate = eventDate.Date;
            while (!workingDaysService.IsWorkingDay(userContext.CourtId, eventDate))
            {
                eventDate = eventDate.AddDays(1).Date;
            }

            return eventDate;
        }

        /// <summary>
        /// Метод извличащ потребител за нотификация от LawUnit
        /// </summary>
        /// <param name="lawUnitId"></param>
        /// <returns></returns>
        private async Task<CaseLawUnitSmallVM> GetaseLawUnitSmallFromLawUnit(int lawUnitId)
        {
            IQueryable<ApplicationUser> applicationUsers = repo.AllReadonly<ApplicationUser>();

            return await repo.AllReadonly<LawUnit>()
                             .Where(x => x.Id == lawUnitId)
                             .Select(x => new CaseLawUnitSmallVM()
                             {
                                 Id = x.Id,
                                 LawUnitId = x.Id,
                                 LawUnitName = x.FullName,
                                 LawUnitUserId = applicationUsers.Where(u => u.LawUnitId == x.Id)
                                                                 .Select(u => u.Id)
                                                                 .FirstOrDefault()
                             })
                             .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Извлича заглавие на нотификация
        /// </summary>
        /// <param name="workNotificationTypeId">Идентификатор на тип нотификация</param>
        /// <returns></returns>
        private async Task<string> GetWorkNotificationTypeTitle(int workNotificationTypeId)
        {
            return await repo.AllReadonly<WorkNotificationType>()
                             .Where(x => x.Id == workNotificationTypeId)
                             .Select(x => x.Description)
                             .FirstOrDefaultAsync();
        }

        #endregion

        #region Новообразувано дело N1

        /// <summary>
        /// Създаване на нотификация за новообразувано дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForN1(int caseId)
        {
            CaseInfoFastProcessVM caseData = await GetDataNewCaseFastProcess(caseId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseId, SourceTypeSelectVM.Case, NomenclatureConstants.WorkNotificationType.N1))
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseId, null, DateTime.Now);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.N1);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.Case,
                    SourceId = caseId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.N1,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    FromUserId = lawUnit.LawUnitUserId,
                    DateCreated = DateTime.Now,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, имате новообразувано електронно заповедно дело с Ваше участие {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Гасене на нотификация за новообразувано дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        public async Task<bool> TurnOffForN1(int caseSessionActId)
        {
            try
            {
                int? caseId = await repo.AllReadonly<CaseSessionAct>()
                                        .Where(x => x.Case.IsFastProcess ?? false)
                                        .Where(x => x.Id == caseSessionActId)
                                        .Where(x => x.ActDeclaredDate != null)
                                        .Select(x => (int?)x.CaseId)
                                        .FirstOrDefaultAsync();

                if (caseId == null)
                    return false;

                return await TurnOfNotificationsForFastProcess(SourceTypeSelectVM.Case, (long)caseId, NomenclatureConstants.WorkNotificationType.N1, "Наличие на постановен съдебен акт.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотификация за новообразувано дело по чл. 410 ГПК или чл. 417 ГПК по акт: {caseSessionActId}");
                return false;
            }


        }

        #endregion

        #region Липса на предприети действия от съдебен служител N3

        /// <summary>
        /// Създаване на нотификация за липса на предприети действия
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <param name="caseSessionResultId">Идентификатор на резултат</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForN3(int? caseSessionActId, int? caseSessionResultId = null)
        {
            List<CaseInfoFastProcessVM> caseDatas = await GetDataTakingActionN3(caseSessionActId, caseSessionResultId);

            List<WorkNotification> notifications = [];

            if (caseDatas.Count < 1)
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseDatas[0].CaseCourtId, caseDatas[0].CaseId, null, DateTime.Now.AddDays(8));
            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.N3);

            foreach (var caseData in caseDatas)
            {
                if (!await ValidateForStartNotificationFastProcess(caseData, (caseData.CaseSessionActId ?? 0), SourceTypeSelectVM.CaseSessionAct, NomenclatureConstants.WorkNotificationType.N3))
                    continue;

                DateTime dateEvent = GetBeginDate((caseData.ActDeclaredDate ?? DateTime.Now).AddDays(8));

                foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
                {
                    notifications.Add(new()
                    {
                        SourceType = SourceTypeSelectVM.CaseSessionAct,
                        SourceId = (caseData.CaseSessionActId ?? 0),
                        CourtId = caseData.CaseCourtId,
                        FromCourtId = caseData.CaseCourtId,
                        CaseId = caseData.CaseId,
                        WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.N3,
                        UserId = lawUnit.LawUnitUserId,
                        IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                        FromUserId = lawUnit.LawUnitUserId,
                        UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                        DateCreated = dateEvent,
                        Title = notificationTitle,
                        Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, не са предприети действия по {caseData.ActTypeLabel.ToLower()} № {caseData.ActRegNumber} по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                        NotificationKind = 2
                    });
                }
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за липса на предприети действия
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForN3(int caseSessionActId)
        {
            return await SaveNotificationsForFastProcess(caseSessionActId, NomenclatureConstants.WorkNotificationType.N3);
        }

        /// <summary>
        /// Запис на нотификация за липса на предприети действия от резултат
        /// </summary>
        /// <param name="caseSessionResultId">Идентификатор на резултат</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForN3Result(int caseSessionResultId)
        {
            return await SaveNotificationsForFastProcess(caseSessionResultId, NomenclatureConstants.WorkNotificationType.N3_From_Result);
        }

        /// <summary>
        /// Гасене на нотификация за липса на предприети действия
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на акт</param>
        /// <param name="saveChanges"></param>
        /// <returns></returns>
        public async Task<bool> TurnOfNotificationsForN3(int caseNotificationId, bool saveChanges = true)
        {
            long[] actIds = await repo.AllReadonly<CaseSessionAct>()
                                      .Where(x => x.DateExpired == null)
                                      .Where(x => x.ActDeclaredDate != null)
                                      .Where(x => x.Case.IsFastProcess ?? false)
                                      .Where(x => x.CaseSession.CaseNotifications.Any(n => n.Id == caseNotificationId))
                                      .Select(x => (long)x.Id)
                                      .ToArrayAsync();

            try
            {
                if (actIds == null || actIds.Length == 0)
                    return true;

                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(n => n.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N3 &&
                                                                             n.DateExpired == null &&
                                                                             n.DateTurnOff == null &&
                                                                             actIds.Contains(n.SourceId) &&
                                                                             n.SourceType == SourceTypeSelectVM.CaseSessionAct)
                                                                 .ToListAsync();

                if (notifications == null || notifications.Count < 1)
                    return true;

                notifications.ForEach(n => { n.DateTurnOff = DateTime.Now; n.DescriptionTurnOff = $"Има изготвено съобщение/призовка/уведомление с идентификатор: {caseNotificationId}"; });
                if (saveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотификации с идентификатори: {string.Join(", ", actIds)} WorkNotificationTypeId:{9}");
                return false;
            }
        }

        #endregion

        #region Предприемане на действия от съдебен служител, при постановяване на съдебен акт N4

        /// <summary>
        /// Създаване на нотификация за предприемане на действия от съдебен служител, при постановяване на съдебен акт
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForActionTakenCourtOfficerDeclatActFastProcess(int caseSessionActId)
        {
            CaseInfoFastProcessVM caseData = await GetDataTakingActionCaseFastProcessN4(caseSessionActId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseSessionActId, SourceTypeSelectVM.CaseSessionAct, NomenclatureConstants.WorkNotificationType.ActionTakenCourtOfficerDeclatActFastProcess))
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitManualRolesFastProcess(caseData.CaseId, null);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.ActionTakenCourtOfficerDeclatActFastProcess);

            foreach (CaseLawUnitSmallVM caseLawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionAct,
                    SourceId = caseSessionActId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.ActionTakenCourtOfficerDeclatActFastProcess,
                    UserId = caseLawUnit.LawUnitUserId,
                    IsUserSubstitution = caseLawUnit.IsLawUnitSubstitution,
                    FromUserId = caseLawUnit.LawUnitUserId,
                    UserCourtDepartmentId = caseLawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = caseData.ActDeclaredDate ?? DateTime.Now,
                    Title = notificationTitle,
                    Description = $"{caseLawUnit.LawUnitName}{(!string.IsNullOrEmpty(caseLawUnit.DepartmentLabel) ? " (" + caseLawUnit.DepartmentLabel + ")" : string.Empty)}, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear} има постановен съдебен акт, по който е необходимо да предприемете действия.",
                    NotificationKind = 2
                });
            }
            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за предприемане на действия от съдебен служител, при постановяване на съдебен акт
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForActionTakenCourtOfficerDeclatActFastProcess(int caseSessionActId)
        {
            return await SaveNotificationsForFastProcess(caseSessionActId, NomenclatureConstants.WorkNotificationType.ActionTakenCourtOfficerDeclatActFastProcess);
        }

        /// <summary>
        /// Гасене на нотификация за предприемане на действия от съдебен служител, при постановяване на съдебен акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на нотификация</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> TurnOfNotificationsForActionTakenCourtOfficerDeclatActFastProcess(int caseNotificationId, bool saveChanges = true)
        {
            long[] actIds = await repo.AllReadonly<CaseSessionAct>()
                                      .Where(x => x.DateExpired == null)
                                      .Where(x => x.ActDeclaredDate != null)
                                      .Where(x => x.Case.IsFastProcess ?? false)
                                      .Where(x => x.CaseSession.CaseNotifications.Any(n => n.Id == caseNotificationId))
                                      .Select(x => (long)x.Id)
                                      .ToArrayAsync();

            try
            {
                if (actIds == null || actIds.Length == 0)
                    return true;


                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(n => n.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.ActionTakenCourtOfficerDeclatActFastProcess &&
                                                                             n.DateExpired == null &&
                                                                             n.DateTurnOff == null &&
                                                                             actIds.Contains(n.SourceId) &&
                                                                             n.SourceType == SourceTypeSelectVM.CaseSessionAct)
                                                                 .ToListAsync();

                if (notifications == null || notifications.Count < 1)
                    return true;

                notifications.ForEach(n => { n.DateTurnOff = DateTime.Now; n.DescriptionTurnOff = $"Има изготвено съобщение/призовка/уведомление с идентификатор: {caseNotificationId}"; });
                if (saveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотификации с идентификатори: {string.Join(", ", actIds)} WorkNotificationTypeId:{28}");
                return false;
            }
        }

        #endregion

        #region Обжалване на акт N6

        /// <summary>
        /// Проверка дали съществува WorkNotification
        /// </summary>
        /// <param name="sourceType">SourceType</param>
        /// <param name="sourceId">SourceId</param>
        /// <param name="workNotificationTypeId">Тип нотификация</param>
        /// <returns></returns>
        private async Task<bool> IsExistsNotificationsCaseFastProcessAppealAct(int sourceType, long sourceId, int caseSessionActId, int workNotificationTypeId)
        {
            return await repo.AllReadonly<WorkNotification>()
                             .AnyAsync(n => n.SourceType == sourceType &&
                                            n.SourceId == sourceId &&
                                            n.ParentId == (long)caseSessionActId &&
                                            n.WorkNotificationTypeId == workNotificationTypeId &&
                                            n.DateExpired == null);
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        private async Task<List<CaseInfoFastProcessVM>> GetDataLackAppealActFastProcess(int caseNotificationId)
        {
            var result = new List<CaseInfoFastProcessVM>();
            var caseNotification = await GetQueryCaseNotification(caseNotificationId).Include(x => x.CaseNotificationActs)
                                                                                     .Include(n => n.CasePerson)
                                                                                     .Where(n => n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier)
                                                                                     .FirstOrDefaultAsync();
            if (caseNotification == null)
            {
                return result;
            }

            var actIds = new List<int>();
            
            if (caseNotification.CaseSessionActId > 0)
            {
                actIds.Add(caseNotification.CaseSessionActId ?? 0);
            }
            
            if (caseNotification.CaseNotificationActs.Any(x => x.IsChecked))
            {
                actIds.AddRange(caseNotification.CaseNotificationActs.Where(x => x.IsChecked).Select(x => x.CaseSessionActId));
            }
            
            if (!actIds.Any())
            {
                return result;
            }

            return await repo.AllReadonly<CaseSessionAct>()
                             .Where(x => actIds.Contains(x.Id))
                             .Where(n => n.CanAppeal ?? false)
                             .Where(n => ((n.AppealNotificationDaysFastProcess ?? 0) > 0 ||
                                         (n.AppealNotificationWeeksFastProcess ?? 0) > 0 ||
                                         (n.AppealNotificationMontsFastProcess ?? 0) > 0))
                             .Select(n => new CaseInfoFastProcessVM()
                             {
                                 CaseId = caseNotification.CaseId,
                                 CaseSessionId = n.CaseSessionId,
                                 CaseCourtId = n.Case.CourtId,
                                 CaseTypeLabel = n.Case.CaseType.Label,
                                 CaseRegNumber = n.Case.RegNumber,
                                 CaseYear = n.Case.RegDate.Year,
                                 NotificationDeliveryPersonFullName = caseNotification.CasePerson.FullName,
                                 DaysFastProcess = n.AppealNotificationDaysFastProcess,
                                 WeeksFastProcess = n.AppealNotificationWeeksFastProcess,
                                 MontsFastProcess = n.AppealNotificationMontsFastProcess,
                                 CaseNotificationId = caseNotificationId,
                                 CaseSessionActId = n.Id
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Извличане на данни за дело
        /// </summary>
        /// <param name="caseSessionAct">Акт</param>
        /// <returns></returns>
        private async Task<List<CaseInfoFastProcessVM>> GetDataLackAppealActFastProcessOnAct(CaseSessionAct caseSessionAct)
        {
            var result = new List<CaseInfoFastProcessVM>();
            var caseNotifications = await repo.AllReadonly<CaseNotification>()
                                              .Where(n => n.CaseSessionActId == caseSessionAct.Id)
                                              .Where(n => n.Case.IsFastProcess ?? false)
                                              .Where(n => n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier)
                                              .Where(n => n.NotificationStateId == NomenclatureConstants.NotificationState.Delivered)
                                              .Include(n => n.CasePerson)
                                              .Include(n => n.Case)
                                              .ToListAsync();

            var caseNotificationsAct = await repo.AllReadonly<CaseNotification>()
                                                 .Where(n => n.CaseNotificationActs.Any(a => a.CaseSessionActId == caseSessionAct.Id && a.IsChecked))
                                                 .Where(n => n.Case.IsFastProcess ?? false)
                                                 .Where(n => n.CasePerson.PersonRoleId != NomenclatureConstants.PersonRole.Notifier)
                                                 .Where(n => n.NotificationStateId == NomenclatureConstants.NotificationState.Delivered)
                                                 .Include(n => n.CasePerson)
                                                 .Include(n => n.Case)
                                                 .ToListAsync();

            var aCase = await repo.AllReadonly<Case>()
                                  .Include(x => x.CaseType)
                                  .Where(x => x.Id == caseSessionAct.CaseId)
                                  .FirstAsync();

            caseNotificationsAct = caseNotificationsAct.Where(x => !caseNotifications.Any(c => c.Id == x.Id)).ToList();
            caseNotifications.AddRange(caseNotificationsAct);

            return caseNotifications.Select(n => new CaseInfoFastProcessVM()
            {
                CaseId = n.CaseId,
                CaseSessionId = n.CaseSessionId,
                CaseCourtId = aCase.CourtId,
                CaseTypeLabel = aCase.CaseType.Label,
                CaseRegNumber = aCase.RegNumber,
                CaseYear = aCase.RegDate.Year,
                NotificationDeliveryPersonFullName = n.CasePerson.FullName,
                DaysFastProcess = caseSessionAct.AppealNotificationDaysFastProcess,
                WeeksFastProcess = caseSessionAct.AppealNotificationWeeksFastProcess,
                MontsFastProcess = caseSessionAct.AppealNotificationMontsFastProcess,
                EventDate = n.DeliveryDate,
                CaseNotificationId = n.Id,
                CaseSessionActId = caseSessionAct.Id
            })
            .ToList();
        }

        /// <summary>
        /// Създаване на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseData">Данни за дело</param>
        /// <param name="caseLawUnits">Списък с лица, за които е нотификацията</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForAppealActFastProcessOne(CaseInfoFastProcessVM caseData, List<CaseLawUnitSmallVM> caseLawUnits)
        {
            List<WorkNotification> notifications = [];
            var caseNotificationId = caseData.CaseNotificationId ?? 0;
            if (!(await IsExistsNotificationsCaseFastProcessAppealAct(SourceTypeSelectVM.CaseNotification, caseNotificationId, caseData.CaseSessionActId ?? 0, NomenclatureConstants.WorkNotificationType.AppealActFastProcess)))
            {
                string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.AppealActFastProcess);

                foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
                {
                    DateTime dateTimeEvent = caseData.EventDate.Value;
                    notifications.Add(new()
                    {
                        SourceType = SourceTypeSelectVM.CaseNotification,
                        SourceId = caseNotificationId,
                        CourtId = caseData.CaseCourtId,
                        ParentId = caseData.CaseSessionActId,
                        FromCourtId = caseData.CaseCourtId,
                        CaseId = caseData.CaseId,
                        WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.AppealActFastProcess,
                        UserId = lawUnit.LawUnitUserId,
                        IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                        FromUserId = lawUnit.LawUnitUserId,
                        UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                        DateCreated = GetBeginDate(dateTimeEvent.AddMonths(caseData.MontsFastProcess ?? 0).AddDays((caseData.WeeksFastProcess ?? 0) * 7).AddDays(caseData.DaysFastProcess ?? 0).AddDays(7)),
                        Title = notificationTitle,
                        Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)},по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear} е изтекъл срокът за обжалване",
                        NotificationKind = 2,
                        DeliveryDate = dateTimeEvent
                    });
                }
            }
            return notifications;
        }

        /// <summary>
        /// Създаване на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForAppealActFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            List<WorkNotification> notifications = [];
            var caseDataList = await GetDataLackAppealActFastProcess(caseNotificationId);
            
            if (!caseDataList.Any())
                return notifications;
            
            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseDataList.First().CaseCourtId, caseDataList.First().CaseId, null, DateTime.Now);
            foreach (var caseData in caseDataList)
            {
                caseData.EventDate = dateTimeEvent;
                notifications.AddRange(await GetNotificationsForAppealActFastProcessOne(caseData, caseLawUnits));
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForAppealActFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            try
            {
                var notifications = await GetNotificationsForAppealActFastProcess(caseNotificationId, dateTimeEvent);
                var caseDataList = await GetDataLackAppealActFastProcess(caseNotificationId);
                var result = false;

                foreach (var caseData in caseDataList)
                {
                    result = await EditDateEventNotificationsForFastProcessActАppeal(notifications, SourceTypeSelectVM.CaseNotification, caseData, NomenclatureConstants.WorkNotificationType.AppealActFastProcess, dateTimeEvent, saveChanges);
                }

                repo.AddRange(notifications);
                if (saveChanges)
                    await repo.SaveChangesAsync();

                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редакция на дата на нотификация тип на обект: {SourceTypeSelectVM.CaseNotification}, идентификатор на обект: {caseNotificationId}, идентификатор на нотификация: {NomenclatureConstants.WorkNotificationType.AppealActFastProcess}");
                return ex.Message;

            }
        }

        /// <summary>
        /// Запис на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseSessionAct">Акт</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task SaveNotificationsForAppealActFastProcessOnActSave(CaseSessionAct caseSessionAct, bool saveChanges)
        {
            try
            {
                List<WorkNotification> notifications = [];
                var caseDataList = await GetDataLackAppealActFastProcessOnAct(caseSessionAct);
                
                if (!caseDataList.Any())
                    return;

                List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseDataList.First().CaseCourtId, caseDataList.First().CaseId, null, DateTime.Now);
                foreach (var caseData in caseDataList)
                {
                    notifications.AddRange(await GetNotificationsForAppealActFastProcessOne(caseData, caseLawUnits));
                }

                var caseDataAct = caseDataList.First();
                await EditDateEventNotificationsForFastProcessActАppeal(notifications, SourceTypeSelectVM.CaseNotification, caseDataAct, NomenclatureConstants.WorkNotificationType.AppealActFastProcess, null, saveChanges);

                repo.AddRange(notifications);
                
                if (saveChanges)
                    await repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редакция на дата на нотификация тип на обект: {SourceTypeSelectVM.CaseNotification}, идентификатор на act: {caseSessionAct.Id}, идентификатор на нотификация: {NomenclatureConstants.WorkNotificationType.AppealActFastProcess}");
            }
        }

        /// <summary>
        /// Сторно на нотификация за обжалване на акт
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForAppealActFastProcess(int caseNotificationId)
        {
            return await ExpiredNotificationsForFastProcess(caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.AppealActFastProcess);
        }

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за връчено съобщение
        /// </summary>
        /// <param name="sourceType">Идентификатор на тип на обекта</param>
        /// <param name="inserted">Списък с нотификации за запис</param>
        /// <param name="caseData">Данни за дело</param>
        /// <param name="workNotificationTypeId">Идентификатор на тип нотификация</param>
        /// <param name="dateTimeEventIn">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        private async Task<bool> EditDateEventNotificationsForFastProcessActАppeal(List<WorkNotification> inserted,
                                                                                   int sourceType,
                                                                                   CaseInfoFastProcessVM caseData,
                                                                                   int workNotificationTypeId,
                                                                                   DateTime? dateTimeEventIn,
                                                                                   bool saveChanges = true)
        {
            if (!inserted.Any() && dateTimeEventIn != null && caseData.CaseNotificationId > 0)
            {
                var dateTimeEvent = dateTimeEventIn?.Date ?? DateTime.Today;
                var dateCreated = GetBeginDate(dateTimeEvent.AddMonths(caseData.MontsFastProcess ?? 0)
                                                            .AddDays((caseData.WeeksFastProcess ?? 0) * 7)
                                                            .AddDays(caseData.DaysFastProcess ?? 0)
                                                            .AddDays(7));

                var workNotifications = await repo.All<WorkNotification>()
                                                  .Where(n => n.SourceType == sourceType)
                                                  .Where(n => n.SourceId == caseData.CaseNotificationId)
                                                  .Where(n => n.ParentId == (long)caseData.CaseSessionActId)
                                                  .Where(n => n.WorkNotificationTypeId == workNotificationTypeId)
                                                  .Where(n => n.DateExpired == null)
                                                  .Where(n => n.DateCreated.Date != dateTimeEvent.Date)
                                                  .ToListAsync();

                workNotifications.ForEach(n => { n.DateCreated = dateCreated; n.DateRead = null; });
            }

            var workNotificationsAll = await repo.All<WorkNotification>()
                                                 .Where(n => n.SourceType == sourceType)
                                                 .Where(n => n.ParentId == (long)caseData.CaseSessionActId)
                                                 .Where(n => n.WorkNotificationTypeId == workNotificationTypeId)
                                                 .Where(n => n.DateExpired == null)
                                                 .ToListAsync();

            workNotificationsAll.AddRange(inserted);
            workNotificationsAll = workNotificationsAll.OrderBy(n => n.DeliveryDate?.Date ?? DateTime.MinValue).ToList();

            if (workNotificationsAll.Any())
            {
                var last = workNotificationsAll.Last();
                var dateTimeEvent = dateTimeEventIn ?? last.DeliveryDate ?? DateTime.Today;
                
                var dateCreated = GetBeginDate(dateTimeEvent.AddMonths(caseData.MontsFastProcess ?? 0)
                                                            .AddDays((caseData.WeeksFastProcess ?? 0) * 7)
                                                            .AddDays(caseData.DaysFastProcess ?? 0)
                                                            .AddDays(7));

                workNotificationsAll.Where(x => x.SourceId != last.SourceId)
                                    .ToList()
                                    .ForEach(n => { n.DateTurnOff = DateTime.Now; n.DateRead = null; });

                workNotificationsAll.Where(x => x.SourceId == last.SourceId)
                                    .ToList()
                                    .ForEach(n =>
                                    {
                                        n.DateTurnOff = null;
                                        n.DateRead = null;
                                        n.DateCreated = dateCreated;
                                        n.DeliveryDate = dateTimeEvent;
                                    });
            }

            if (saveChanges)
                await repo.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Връчено съобщение N7

        /// <summary>
        /// Метод връщащ брой дни за добавяне за нотификация N7
        /// </summary>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        private int GetAddDaysN7(DateTime dateTimeEvent)
        {
            DateTime _now = DateTime.Now;

            int dayAdd = (_now.Date - dateTimeEvent.Date).Days;

            return dayAdd < 0 ? 0 : (dayAdd > 5 ? 5 : dayAdd);
        }

        /// <summary>
        /// Метод връщащ дата за нотификация N7
        /// </summary>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        private DateTime GetDateCreateN7(DateTime dateTimeEvent)
        {
            DateTime _now = DateTime.Now;

            int dayAdd = (_now.Date - dateTimeEvent.Date).Days;

            return dayAdd > 5 ? _now : dateTimeEvent.AddDays(5);
        }

        /// <summary>
        /// Създаване на нотификация за връчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForMessageDeliveredCaseFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            var caseData = await GetDataMessageDeliveredFastProcess(caseNotificationId, true);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.MessageDeliveredFastProcess))
                return notifications;

            DateTime dateTime = GetDateCreateN7(dateTimeEvent);

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, dateTime);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.MessageDeliveredFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.MessageDeliveredFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, има връчено съобщение, което е отразено като връчено на дата {dateTimeEvent.ToString("dd.MM.yyyy")} на {caseData.NotificationDeliveryPersonFullName}, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за връчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForMessageDeliveredCaseFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            return await SaveNotificationsForFastProcess(caseNotificationId, NomenclatureConstants.WorkNotificationType.MessageDeliveredFastProcess, saveChanges, dateTimeEvent);
        }

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за връчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> EditDateEventNotificationsForMessageDeliveredCaseFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true)
        {
            return await EditDateEventNotificationsForFastProcess(SourceTypeSelectVM.CaseNotification, caseNotificationId, NomenclatureConstants.WorkNotificationType.MessageDeliveredFastProcess, dateTimeEvent.Date.AddDays(5), saveChanges);
        }

        /// <summary>
        /// Сторно на нотификация за връчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForMessageDeliveredCaseFastProcess(int caseNotificationId)
        {
            return await ExpiredNotificationsForFastProcess(caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.MessageDeliveredFastProcess);
        }

        #endregion

        #region Невръчено съобщение N8

        /// <summary>
        /// Създаване на нотификация за невръчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotDeliveredMessageDeliveryFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            var caseData = await GetDataMessageDeliveredFastProcess(caseNotificationId, true);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.NotDeliveredMessageDeliveryFastProcess))
                return notifications;

            DateTime dateTime = DateTime.Now;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, dateTime);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.NotDeliveredMessageDeliveryFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.NotDeliveredMessageDeliveryFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, постъпило е съобщение за невръчване по {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за невръчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForNotDeliveredMessageDeliveryFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            return await SaveNotificationsForFastProcess(caseNotificationId, NomenclatureConstants.WorkNotificationType.NotDeliveredMessageDeliveryFastProcess, saveChanges, dateTimeEvent);
        }

        /// <summary>
        /// Сторно на нотификация за невръчено съобщение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForNotDeliveredMessageDeliveryFastProcess(int caseNotificationId)
        {
            return await ExpiredNotificationsForFastProcess(caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.NotDeliveredMessageDeliveryFastProcess);
        }

        #endregion

        #region Залепено уведомление N10

        /// <summary>
        /// Създаване на нотификация за залепено уведомление
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForStuckMessagesFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            var caseData = await GetDataMessageDeliveredFastProcess(caseNotificationId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.StuckMessagesFastProcess))
                return notifications;

            DateTime dateTime = GetBeginDate(dateTimeEvent.AddDays(14));

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, dateTime);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.StuckMessagesFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.StuckMessagesFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, има залепено съобщение на {caseData.NotificationDeliveryPersonFullName}, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за залепено уведомление
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForStuckMessagesFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            return await SaveNotificationsForFastProcess(caseNotificationId, NomenclatureConstants.WorkNotificationType.StuckMessagesFastProcess, saveChanges, dateTimeEvent);
        }

        /// <summary>
        /// Сторно на нотификация за залепено уведомление
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForStuckMessagesFastProcess(int caseNotificationId)
        {
            return await ExpiredNotificationsForFastProcess(caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.StuckMessagesFastProcess);
        }

        #endregion

        #region Постановен влязъл в сила финализиращ акт от друга инстанция /при обжалване на акт на заповедния съд/въззивни производства по чл. 413, 419, 420 и 423 от ГПК N11

        /// <summary>
        /// Създаване на нотификация за постановен влязъл в сила финализиращ акт от друга инстанция /при обжалване на акт на заповедния съд/въззивни производства по чл. 413, 419, 420 и 423 от ГПК
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForN11(int caseSessionActId)
        {
            CaseInfoFastProcessVM caseData = await GetDataActDeclaredAnotherInstanceFastProcess(caseSessionActId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseSessionActId, SourceTypeSelectVM.CaseSessionAct, NomenclatureConstants.WorkNotificationType.N11))
                return notifications;

            DateTime dateTime = caseData.ActDeclaredDate ?? DateTime.Now;

            List<CaseLawUnitSmallVM> judgeReporters = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, dateTime);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.N11);

            foreach (CaseLawUnitSmallVM judgeReporter in judgeReporters)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionAct,
                    SourceId = caseSessionActId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.N11,
                    UserId = judgeReporter.LawUnitUserId,
                    IsUserSubstitution = judgeReporter.IsLawUnitSubstitution,
                    FromUserId = judgeReporter.LawUnitUserId,
                    UserCourtDepartmentId = judgeReporter.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"{judgeReporter.LawUnitName}{(!string.IsNullOrEmpty(judgeReporter.DepartmentLabel) ? " (" + judgeReporter.DepartmentLabel + ")" : string.Empty)}, има постановен финализиращ акт на горна инстанция, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за постановен влязъл в сила финализиращ акт от друга инстанция /при обжалване на акт на заповедния съд/въззивни производства по чл. 413, 419, 420 и 423 от ГПК
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForN11(int caseSessionActId)
        {
            return await SaveNotificationsForFastProcess(caseSessionActId, NomenclatureConstants.WorkNotificationType.N11);
        }

        #endregion

        #region Известяване за влязъл в сила акт от друга инстанция N12

        /// <summary>
        /// Създаване на нотификация за известяване за влязъл в сила акт от друга инстанция
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForActInforcedAnotherInstanceFastProcess(int caseSessionActId)
        {
            CaseInfoFastProcessVM caseData = await GetDataActInforcedАnotherInstanceFastProcess(caseSessionActId, true);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseSessionActId, SourceTypeSelectVM.CaseSessionAct, NomenclatureConstants.WorkNotificationType.ActInforcedAnotherInstanceFastProcess))
                return notifications;

            DateTime dateTime = caseData.ActInforcedDate ?? DateTime.Now;

            List<CaseLawUnitSmallVM> judgeReporters = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, dateTime);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.ActInforcedAnotherInstanceFastProcess);

            foreach (CaseLawUnitSmallVM judgeReporter in judgeReporters)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionAct,
                    SourceId = caseSessionActId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.ActInforcedAnotherInstanceFastProcess,
                    UserId = judgeReporter.LawUnitUserId,
                    IsUserSubstitution = judgeReporter.IsLawUnitSubstitution,
                    FromUserId = judgeReporter.LawUnitUserId,
                    UserCourtDepartmentId = judgeReporter.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"{judgeReporter.LawUnitName}{(!string.IsNullOrEmpty(judgeReporter.DepartmentLabel) ? " (" + judgeReporter.DepartmentLabel + ")" : string.Empty)}, има влязъл в сила финализиращ акт, по исково дело по чл. 422/424 от ГПК {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за известяване за влязъл в сила акт от друга инстанция
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForActInforcedAnotherInstanceFastProcess(int caseSessionActId)
        {
            return await SaveNotificationsForFastProcess(caseSessionActId, NomenclatureConstants.WorkNotificationType.ActInforcedAnotherInstanceFastProcess);
        }

        #endregion

        #region Липса на подадено в срок възражение N13

        /// <summary>
        /// Създаване на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForLackSubmittedObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            var caseData = await GetDataLackSubmittedObjectionFastProcess(caseNotificationId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess))
                return notifications;

            DateTime dateTime = GetBeginDate(dateTimeEvent.AddMonths(1).AddDays(7));

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, dateTime);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, липсва подадено в срок възражение, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForLackSubmittedObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            return await SaveNotificationsForFastProcess(caseNotificationId, NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess, saveChanges, dateTimeEvent);
        }

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> EditDateEventNotificationsForLackSubmittedObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true)
        {
            DateTime _date = GetBeginDate(dateTimeEvent.AddMonths(1).AddDays(7));
            return await EditDateEventNotificationsForFastProcess(SourceTypeSelectVM.CaseNotification, caseNotificationId, NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess, _date, saveChanges);
        }

        /// <summary>
        /// Сторно на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForLackSubmittedObjectionFastProcess(int caseNotificationId)
        {
            return await ExpiredNotificationsForFastProcess(caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess);
        }

        /// <summary>
        /// Гасене на нотификация за липса на подадено в срок възражение
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<bool> TurnOffNotificationsForLackSubmittedObjectionFastProcess(int caseId)
        {
            var caseMigrationData = await repo.AllReadonly<CaseMigration>()
                                                  .Where(x => x.CaseId == caseId)
                                                  .OrderByDescending(x => x.DateWrt)
                                                  .Select(x => new
                                                  {
                                                      x.PriorCaseId,
                                                      x.CaseMigrationType.MigrationDirection
                                                  })
                                                  .FirstOrDefaultAsync();

            if (caseMigrationData == null)
                return false;

            if (caseMigrationData.MigrationDirection < 2)
                return false;
            
            long[] caseNotificationIds = await repo.AllReadonly<CaseNotification>()
                                                   .Where(x => x.CaseId == caseMigrationData.PriorCaseId)
                                                   .Where(x => x.DateExpired == null)
                                                   .Select(x => (long)x.Id)
                                                   .ToArrayAsync();

            try
            {
                if (caseNotificationIds == null || caseNotificationIds.Length == 0)
                    return true;

                List<WorkNotification> notifications = await repo.All<WorkNotification>()
                                                                 .Where(n => n.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess &&
                                                                             n.DateExpired == null &&
                                                                             n.DateTurnOff == null &&
                                                                             caseNotificationIds.Contains(n.SourceId) &&
                                                                             n.SourceType == SourceTypeSelectVM.CaseNotification)
                                                                 .ToListAsync();

                if (notifications == null || notifications.Count < 1)
                    return true;

                notifications.ForEach(n => { n.DateTurnOff = DateTime.Now; n.DescriptionTurnOff = $"Има образувано искове производство"; });
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотификация sourceType:{6} sourceIds: {string.Join(",", caseNotificationIds)} WorkNotificationTypeId:{13}");
                return false;
            }
        }

        /// <summary>
        /// Гасене на нотификация за липса на подадено в срок възражение от документ с основен вид възражение
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> TurnOffNotificationsObjectionForLackSubmittedObjectionFastProcess(long documentId, bool saveChanges = true)
        {
            int? caseId = await GetCaseIdForLackSubmittedObjectionFastProcess(documentId);

            if (caseId == null)
                return false;

            return await TurnOfNotificationsForFastProcess(caseId ?? 0, NomenclatureConstants.WorkNotificationType.LackSubmittedObjectionFastProcess, "Има входиран съпровождащ документ от вид възражение", saveChanges);
        }

        #endregion

        #region Липса на предприето процесуално действие от страна N14

        /// <summary>
        /// Метод връщащ списък със създадени нотификации за предприето процесуално действие от страна
        /// </summary>
        /// <param name="caseSessionId">Идентификатор на заседание</param>
        /// <returns></returns>
        private async Task<List<WorkNotification>> GetSaveNotificationNoProceduralActionTakenFastProcess(int caseSessionId)
        {
            IQueryable<CaseNotification> caseNotifications = repo.AllReadonly<CaseNotification>();

            return await repo.All<WorkNotification>()
                             .Where(n => n.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.NoProceduralActionTakenFastProcess)
                             .Where(n => n.DateRead == null)
                             .Where(n => n.DateExpired == null)
                             .Where(n => caseNotifications.Any(w => w.Id == (int)n.SourceId &&
                                                                    w.CaseSessionId == caseSessionId))
                             .ToListAsync();
        }

        /// <summary>
        /// Метод връщащ списък със създадени нотификации N23
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        private async Task<List<WorkNotification>> GetSaveNotificationN23(int caseSessionActId)
        {
            IQueryable<CaseNotification> caseNotifications = repo.AllReadonly<CaseNotification>();

            long actId = (long)caseSessionActId;

            return await repo.All<WorkNotification>()
                             .Where(n => n.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.N23)
                             .Where(n => n.DateRead == null)
                             .Where(n => n.DateExpired == null)
                             .Where(n => n.SourceType == SourceTypeSelectVM.CaseSessionAct)
                             .Where(n => n.SourceId == actId)
                             .ToListAsync();
        }

        /// <summary>
        /// Създаване/редактиране на нотификация за липса на предприето процесуално действие от страна
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        public async Task<string> SaveUpdateNotificationsForNoProceduralActionTakenFastProcess(int caseSessionActId, bool saveChanges = true)
        {
            try
            {
                List<CaseInfoFastProcessVM> caseDatas = await GetDataFromActNoProceduralActionTakenFastProcess(caseSessionActId);

                if (caseDatas == null || caseDatas.Count == 0)
                    return string.Empty;

                List<WorkNotification> workNotifications = await GetSaveNotificationNoProceduralActionTakenFastProcess(caseDatas[0].CaseSessionId ?? 0);

                if (workNotifications != null && workNotifications.Any())
                {
                    foreach (WorkNotification notification in workNotifications)
                    {
                        if (caseDatas[0].CaseSessionActNotificationOn)
                            notification.DateCreated = GetBeginDate((notification.InitialDate ?? DateTime.Now).AddMonths(caseDatas[0].MontsFastProcess ?? 0).AddDays((caseDatas[0].WeeksFastProcess ?? 0) * 7).AddDays(caseDatas[0].DaysFastProcess ?? 0));
                        else
                        {
                            notification.DateExpired = DateTime.Now;
                            notification.UserExpiredId = userContext.UserId;
                        }

                        repo.Update(notification);
                    }
                }
                else
                {
                    List<WorkNotification> notifications = [];

                    List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseDatas[0].CaseCourtId, caseDatas[0].CaseId, null, DateTime.Now);

                    string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.NoProceduralActionTakenFastProcess);

                    foreach (var caseData in caseDatas)
                    {
                        foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
                        {
                            notifications.Add(new()
                            {
                                SourceType = SourceTypeSelectVM.CaseNotification,
                                SourceId = caseData?.CaseNotificationId ?? 0,
                                CourtId = caseData.CaseCourtId,
                                FromCourtId = caseData.CaseCourtId,
                                CaseId = caseData.CaseId,
                                WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.NoProceduralActionTakenFastProcess,
                                UserId = lawUnit.LawUnitUserId,
                                IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                                FromUserId = lawUnit.LawUnitUserId,
                                UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                                DateCreated = GetBeginDate((caseData.EventDate ?? DateTime.Now).AddMonths(caseData.MontsFastProcess ?? 0).AddDays((caseData.WeeksFastProcess ?? 0) * 7).AddDays(caseData.DaysFastProcess ?? 0)),
                                InitialDate = (caseData.EventDate ?? DateTime.Now),
                                Title = notificationTitle,
                                Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, има липса за предприемане на процесуално действие, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                                NotificationKind = 2
                            });
                        }
                    }

                    repo.AddRange(notifications);
                }

                if (saveChanges)
                    await repo.SaveChangesAsync();

                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при добавяне/редакция на нотификация за липса на предприето процесуално действие от страна sourceType: {7} sourceId: {caseSessionActId}");
                return ex.Message;
            }
        }

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за липса на предприето процесуално действие от страна
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> EditDateEventNotificationsForNoProceduralActionTakenFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true)
        {
            return await EditDateEventNotificationsForFastProcess(SourceTypeSelectVM.CaseNotification, caseNotificationId, NomenclatureConstants.WorkNotificationType.NoProceduralActionTakenFastProcess, dateTimeEvent.Date.AddDays(1), saveChanges);
        }

        #endregion

        #region Изтекъл срок за изразяване на становище по възражение по чл. 414 а ГПК N15

        /// <summary>
        /// Създаване на нотификация за изтекъл срок за изразяване на становище по възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForExpressingOpinionObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            var caseData = await GetDataLackExpressingOpinionObjectionFastProcess(caseNotificationId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.ExpressingOpinionObjectionFastProcess))
                return notifications;

            DateTime dateTime = GetBeginDate(dateTimeEvent.AddDays(10));

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, dateTime);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.ExpressingOpinionObjectionFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.ExpressingOpinionObjectionFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, изтекъл е срокът за изразяване на становище по възражение по чл. 414 а ГПК, по {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за изтекъл срок за изразяване на становище по възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForExpressingOpinionObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            return await SaveNotificationsForFastProcess(caseNotificationId, NomenclatureConstants.WorkNotificationType.ExpressingOpinionObjectionFastProcess, saveChanges);
        }

        /// <summary>
        /// Сторно на нотификация за изтекъл срок за изразяване на становище по възражение
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForExpressingOpinionObjectionFastProcess(long documentId, bool saveChanges = true)
        {
            int? caseId = await GetCaseIdForExpiredNotificationsForExpressingOpinionObjectionFastProcess(documentId);

            if (caseId == null)
                return false;

            return await ExpiredNotificationsForFastProcess(caseId ?? 0, NomenclatureConstants.WorkNotificationType.ExpressingOpinionObjectionFastProcess, saveChanges);
        }

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за изтекъл срок за изразяване на становище по възражение
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> EditDateEventNotificationsForExpressingOpinionObjectionFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true)
        {
            DateTime _date = GetBeginDate(dateTimeEvent.AddDays(10));
            return await EditDateEventNotificationsForFastProcess(SourceTypeSelectVM.CaseNotification, caseNotificationId, NomenclatureConstants.WorkNotificationType.ExpressingOpinionObjectionFastProcess, _date, saveChanges);
        }

        #endregion

        #region Изтекъл срок за предявяване на иск по чл. 422 ГПК N16

        /// <summary>
        /// Създаване на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForFilingClaimFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            var caseData = await GetDataFilingClaimFastProcess(caseNotificationId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.FilingClaimFastProcess))
                return notifications;

            DateTime dateTime = GetBeginDate(dateTimeEvent.AddMonths(1).AddDays(7));

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, dateTime);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.FilingClaimFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.FilingClaimFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, изтекъл е срокът за предявяване на иск по чл. 422 ГПК, свързан с {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForFilingClaimFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            return await SaveNotificationsForFastProcess(caseNotificationId, NomenclatureConstants.WorkNotificationType.FilingClaimFastProcess, saveChanges);
        }

        /// <summary>
        /// Сторно на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForFilingClaimFastProcess(int caseId, bool saveChanges = true)
        {
            int? _caseId = await GetValidateCaseIdExpiredNotificationsForFilingClaimFastProcess(caseId);
            if ((_caseId ?? 0) < 1)
                return false;

            return await ExpiredNotificationsForFastProcess(_caseId ?? 0, NomenclatureConstants.WorkNotificationType.FilingClaimFastProcess, saveChanges);
        }

        /// <summary>
        /// Гасене на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> TurnOffNotificationsForFilingClaimFastProcess(int caseId, bool saveChanges = true)
        {
            int? _caseId = await GetValidateCaseIdExpiredNotificationsForFilingClaimFastProcess(caseId);
            if ((_caseId ?? 0) < 1)
                return false;

            return await TurnOfNotificationsForFastProcess(_caseId ?? 0, NomenclatureConstants.WorkNotificationType.FilingClaimFastProcess, null, saveChanges);
        }

        /// <summary>
        /// Редакция на дата на стартиране на нотификация за изтекъл срок за предявяване на иск по чл. 422 ГПК
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> EditDateEventNotificationsForFilingClaimFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges = true)
        {
            DateTime _date = GetBeginDate(dateTimeEvent.AddMonths(1).AddDays(7));
            return await EditDateEventNotificationsForFastProcess(SourceTypeSelectVM.CaseNotification, caseNotificationId, NomenclatureConstants.WorkNotificationType.FilingClaimFastProcess, _date, saveChanges);
        }

        #endregion

        #region Потвърждение за подаване на иск по чл. 422 N17

        /// <summary>
        /// Създаване на нотификация за образувано исково дело по чл. 422 ГПК свързано с дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess(int caseId)
        {
            CaseInfoFastProcessVM caseData = await GetDataNewCaseHigherInstanceFastProcess(caseId, true);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseId, SourceTypeSelectVM.Case, NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWith0604_1_2FastProcess))
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, caseData.CaseRegDate);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWith0604_1_2FastProcess);

            foreach (CaseLawUnitSmallVM caseLawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.Case,
                    SourceId = caseId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWith0604_1_2FastProcess,
                    UserId = caseLawUnit.LawUnitUserId,
                    IsUserSubstitution = caseLawUnit.IsLawUnitSubstitution,
                    FromUserId = caseLawUnit.LawUnitUserId,
                    UserCourtDepartmentId = caseLawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = caseData.CaseRegDate,
                    Title = notificationTitle,
                    Description = $"{caseLawUnit.LawUnitName}{(!string.IsNullOrEmpty(caseLawUnit.DepartmentLabel) ? " (" + caseLawUnit.DepartmentLabel + ")" : string.Empty)}, потвърждение за подаване на иск по чл. 422 {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за образувано исково дело по чл. 422 ГПК свързано с дело по чл. 410 ГПК или чл. 417 ГПК
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForNewCaseHigherInstanceWith0604_1_2FastProcess(int caseId)
        {
            return await SaveNotificationsForFastProcess(caseId, NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWith0604_1_2FastProcess);
        }

        #endregion

        #region Известяване за получено съобщение за връчване N18

        /// <summary>
        /// Създаване на нотификация за известяване за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForNoticeServiceReceivedFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            var caseData = await GetDataMessageDeliveredFastProcessWithSummons(caseNotificationId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.NoticeServiceReceivedFastProcess))
                return notifications;

            if (caseData.NotificationLawUnitId == null || caseData.NotificationLawUnitId < 1)
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = [await GetaseLawUnitSmallFromLawUnit(caseData.NotificationLawUnitId ?? 0)];

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.NoticeServiceReceivedFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.NoticeServiceReceivedFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTimeEvent,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, има получено съобщение за връчване {(string.IsNullOrEmpty(caseData.NotificationToCourtLabel) ? string.Empty : "от " + caseData.NotificationToCourtLabel + " ")}на {caseData.NotificationDeliveryPersonFullName}, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за известяване за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForNoticeServiceReceivedFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            return await SaveNotificationsForFastProcess(caseNotificationId, NomenclatureConstants.WorkNotificationType.NoticeServiceReceivedFastProcess, saveChanges, dateTimeEvent);
        }

        /// <summary>
        /// Сторно на нотификация за известяване за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForNoticeServiceReceivedFastProcess(int caseNotificationId)
        {
            return await ExpiredNotificationsForFastProcess(caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.NoticeServiceReceivedFastProcess);
        }

        #endregion

        #region Връчен изпълнителен лист от съдебен изпълнител N19

        /// <summary>
        /// Създаване на нотификация за връчен изпълнителен лист от съдебен изпълнител
        /// </summary>
        /// <param name="sourceType">Тип на обекта</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="description">Описание</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForDeliveredЕxecutiveListFastProcess(int sourceType, long sourceId, DateTime dateTimeEvent, string description = null)
        {
            CaseInfoFastProcessVM caseData = await GetDataDeliveredЕxecutiveListFastProcess(sourceType, sourceId);

            List<WorkNotification> notifications = [];

            if (caseData == null)
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, DateTime.Now);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.DeliveredЕxecutiveListFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = sourceType,
                    SourceId = sourceId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.DeliveredЕxecutiveListFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = DateTime.Now,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, има връчен изпълнителен лист {caseData.ExecListNumber} на {dateTimeEvent.ToString("dd.MM.yyyy")}, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за връчен изпълнителен лист от съдебен изпълнител
        /// </summary>
        /// <param name="sourceType">Тип на обекта</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="description">Описание</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForDeliveredЕxecutiveListFastProcess(int sourceType, long sourceId, DateTime dateTimeEvent, string description = null, bool saveChanges = true)
        {
            try
            {
                List<WorkNotification> notifications = [];
                dateTimeEvent = dateTimeEvent < new DateTime(1900, 1, 1) ? DateTime.Now : dateTimeEvent;

                notifications = await GetNotificationsForDeliveredЕxecutiveListFastProcess(sourceType, sourceId, dateTimeEvent, description);

                if (notifications != null && notifications.Count() > 0)
                {
                    repo.AddRange(notifications);
                    if (saveChanges)
                        await repo.SaveChangesAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на нотификация за връчен изпълнителен лист от съдебен изпълнител sourceType: {sourceType} sourceId: {sourceId}");
                return ex.Message;
            }
        }

        #endregion

        #region Постановен акт за отвод/самоотвод N20

        /// <summary>
        /// Създаване на нотификация за постановен акт за отвод/самоотвод
        /// </summary>
        /// <param name="actId">Идентификатор на акт</param>
        /// <param name="sessionResultId">Идентификатор на резултат от заседание</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForDecreeRecusalSelfRecusalFastProcess(int? actId, int? sessionResultId)
        {
            List<WorkNotification> notifications = [];

            if (actId == null && sessionResultId == null)
                return notifications;

            var caseData = actId != null ? await GetDataDecreeRecusalSelfRecusalFastProcessByActId(actId ?? 0) :
                                           await GetDataDecreeRecusalSelfRecusalFastProcessBySessionResultId(sessionResultId ?? 0);

            if (!await ValidateForStartNotificationFastProcess(caseData, caseData?.CaseId ?? 0, SourceTypeSelectVM.Case, NomenclatureConstants.WorkNotificationType.DecreeRecusalSelfRecusalFastProcess))
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitManualRolesFastProcess(caseData.CaseId, null);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.DecreeRecusalSelfRecusalFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.Case,
                    SourceId = caseData.CaseId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.DecreeRecusalSelfRecusalFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = caseData.ActDeclaredDate ?? DateTime.Now,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, има постановен акт за отвод/самоотвод по {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за постановен акт за отвод/самоотвод
        /// </summary>
        /// <param name="actId">Идентификатор на акта</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForDecreeRecusalSelfRecusalFastProcessByActId(int actId, bool saveChanges = true)
        {
            try
            {
                List<WorkNotification> notifications = [];

                notifications = await GetNotificationsForDecreeRecusalSelfRecusalFastProcess(actId, null);

                if (notifications != null && notifications.Count() > 0)
                {
                    repo.AddRange(notifications);
                    if (saveChanges)
                        await repo.SaveChangesAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на нотификация на акт:{actId} WorkNotificationTypeId: {NomenclatureConstants.WorkNotificationType.DecreeRecusalSelfRecusalFastProcess}");
                return ex.Message;
            }
        }

        /// <summary>
        /// Запис на нотификация за постановен акт за отвод/самоотвод
        /// </summary>
        /// <param name="sessionResultId">Идентификатор на резултат от заседание</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForDecreeRecusalSelfRecusalFastProcessBySessionResultId(int sessionResultId, bool saveChanges = true)
        {
            try
            {
                List<WorkNotification> notifications = [];

                notifications = await GetNotificationsForDecreeRecusalSelfRecusalFastProcess(null, sessionResultId);

                if (notifications != null && notifications.Count() > 0)
                {
                    repo.AddRange(notifications);
                    if (saveChanges)
                        await repo.SaveChangesAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на нотификация на резултат от заседание:{sessionResultId} WorkNotificationTypeId: {NomenclatureConstants.WorkNotificationType.DecreeRecusalSelfRecusalFastProcess}");
                return ex.Message;
            }
        }

        #endregion

        #region Постъпване на съпровождащ документ N21

        /// <summary>
        /// Създаване на нотификация за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForCompliantDocumentCaseFastProcess(long documentId)
        {
            var caseData = await GetDataCompliantDocumentCaseFastProcess(documentId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, documentId, SourceTypeSelectVM.Document, NomenclatureConstants.WorkNotificationType.CompliantDocumentCaseFastProcess))
                return notifications;

            List<CaseLawUnitSmallVM> judgeReporters = await GetCaseLawUnitAllFastProcess(0, caseData.CaseCourtId, caseData.CaseId, null, caseData.DocumentDate ?? DateTime.Now);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.CompliantDocumentCaseFastProcess);

            foreach (CaseLawUnitSmallVM judgeReporter in judgeReporters)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.Document,
                    SourceId = documentId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.CompliantDocumentCaseFastProcess,
                    UserId = judgeReporter.LawUnitUserId,
                    IsUserSubstitution = judgeReporter.IsLawUnitSubstitution,
                    UserCourtDepartmentId = judgeReporter.DepartmentId.NumberEmptyToNull(),
                    FromUserId = judgeReporter.LawUnitUserId,
                    DateCreated = caseData.DocumentDate ?? DateTime.Now,
                    Title = notificationTitle,
                    Description = $"{judgeReporter.LawUnitName}{(!string.IsNullOrEmpty(judgeReporter.DepartmentLabel) ? " (" + judgeReporter.DepartmentLabel + ")" : string.Empty)}, постъпил е съпровождащ документ по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="documentId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForCompliantDocumentCaseFastProcess(long documentId)
        {
            return await SaveNotificationsForFastProcess(documentId, NomenclatureConstants.WorkNotificationType.CompliantDocumentCaseFastProcess);
        }

        /// <summary>
        /// Метод извличащ записани нотификации за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        private async Task<List<WorkNotification>> GetSaveNotificationsForCompliantDocumentCaseFastProcess(long documentId)
        {
            return await repo.All<WorkNotification>()
                             .Where(x => x.SourceId == documentId)
                             .Where(x => x.SourceType == SourceTypeSelectVM.Document)
                             .Where(x => x.WorkNotificationTypeId == NomenclatureConstants.WorkNotificationType.CompliantDocumentCaseFastProcess)
                             .Where(x => x.DateExpired == null)
                             .ToListAsync();
        }

        /// <summary>
        /// Метод коригиращ нотификация за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        public async Task<bool> ExpiredEditNotificationsForCompliantDocumentCaseFastProcess(long documentId)
        {
            try
            {
                List<WorkNotification> workNotifications = await GetSaveNotificationsForCompliantDocumentCaseFastProcess(documentId);

                if (workNotifications == null || !workNotifications.Any())
                    return true;

                var caseData = await GetDataCompliantDocumentCaseFastProcess(documentId);

                if (caseData == null)
                {
                    workNotifications.ForEach(n => { n.DateExpired = DateTime.Now; n.UserExpiredId = userContext.UserId; });
                    await repo.SaveChangesAsync();
                    await SaveNotificationsForCompliantDocumentCaseFastProcess(documentId);
                }
                else
                {
                    if (workNotifications.Any(n => n.CaseId != caseData.CaseId))
                    {
                        workNotifications.Where(n => n.CaseId != caseData.CaseId)
                                         .ToList()
                                         .ForEach(n =>
                                         {
                                             n.DateExpired = DateTime.Now;
                                             n.UserExpiredId = userContext.UserId;
                                         });
                        await repo.SaveChangesAsync();
                        await SaveNotificationsForCompliantDocumentCaseFastProcess(documentId);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при коригиране на нотификация за постъпване на съпровождащ документ: {1} sourceId: {documentId}");
                return false;
            }
        }

        /// <summary>
        /// Метод за гасена на нотификация за постъпване на съпровождащ документ
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <param name="isSaveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> TurnOfCompliantDocumentCaseFastProcess(int caseSessionActId,bool isSaveChanges)
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
                    await TurnOfNotificationsForFastProcess(SourceTypeSelectVM.Document, docId.DocumentId, NomenclatureConstants.WorkNotificationType.CompliantDocumentCaseFastProcess, "Има постановен акт", false);
                }

                if (isSaveChanges)
                    await repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотификация за постъпване на съпровождащ документ. Идентификатор на акт: {caseSessionActId}");
                return false;
            }
        }

        #endregion

        #region Образуване на свързано дело на горна инстанция N22

        /// <summary>
        /// Създаване на нотификация за образуване на свързано дело на горна инстанция
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForNewCaseHigherInstanceWithout0604_1_2FastProcess(int caseId)
        {
            CaseInfoFastProcessVM caseData = await GetDataNewCaseHigherInstanceFastProcess(caseId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseId, SourceTypeSelectVM.Case, NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWithout0604_1_2FastProcess))
                return notifications;

            List<CaseLawUnitSmallVM> judgeReporters = await GetJudgeReporterFastProcess(caseData.CaseCourtId, caseData.CaseId, null, caseData.CaseRegDate);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWithout0604_1_2FastProcess);

            foreach (CaseLawUnitSmallVM judgeReporter in judgeReporters)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.Case,
                    SourceId = caseId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWithout0604_1_2FastProcess,
                    UserId = judgeReporter.LawUnitUserId,
                    IsUserSubstitution = judgeReporter.IsLawUnitSubstitution,
                    FromUserId = judgeReporter.LawUnitUserId,
                    UserCourtDepartmentId = judgeReporter.DepartmentId.NumberEmptyToNull(),
                    DateCreated = caseData.CaseRegDate,
                    Title = notificationTitle,
                    Description = $"{judgeReporter.LawUnitName}{(!string.IsNullOrEmpty(judgeReporter.DepartmentLabel) ? " (" + judgeReporter.DepartmentLabel + ")" : string.Empty)}, има новообразувано свързано дело на горна инстанция, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за образуване на свързано дело на горна инстанция
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForNewCaseHigherInstanceWithout0604_1_2FastProcess(int caseId)
        {
            return await SaveNotificationsForFastProcess(caseId, NomenclatureConstants.WorkNotificationType.NewCaseHigherInstanceWithout0604_1_2FastProcess);
        }

        #endregion

        #region Изтекъл, определения от потребителя, срок от постановяване на акт N23

        /// <summary>
        /// Създаване на нотификация N23
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForN23(int caseSessionActId)
        {
            CaseInfoFastProcessVM caseData = await GetDataN23(caseSessionActId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseSessionActId, SourceTypeSelectVM.CaseSessionAct, NomenclatureConstants.WorkNotificationType.N23))
                return notifications;

            DateTime dateTime = (caseData.ActDeclaredDate ?? DateTime.Now).AddMonths(caseData.MontsFastProcess ?? 0).AddDays((caseData.WeeksFastProcess ?? 0) * 7).AddDays(caseData.DaysFastProcess ?? 0);

            List<CaseLawUnitSmallVM> lawUnits = await GetCaseLawUnitManualRolesFastProcess(caseData.CaseId, null);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.N23);

            foreach (CaseLawUnitSmallVM lawUnit in lawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseSessionAct,
                    SourceId = caseSessionActId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.N23,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    InitialDate = caseData.ActDeclaredDate,
                    DateCreated = dateTime,
                    Title = notificationTitle,
                    Description = $"Предприемане на действия по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}{(string.IsNullOrEmpty(caseData.Description) ? " - " + caseData.Description : string.Empty)}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация N23
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акта</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForN23(int caseSessionActId)
        {
            return await SaveNotificationsForFastProcess(caseSessionActId, NomenclatureConstants.WorkNotificationType.N23);
        }

        /// <summary>
        /// Редактиране на нотификация N23
        /// </summary>
        /// <param name="caseSessionActId">Идентификатор на акт</param>
        /// <returns></returns>
        public async Task<string> SaveUpdateNotificationsForN23(int caseSessionActId, bool saveChanges = true)
        {
            try
            {
                CaseInfoFastProcessVM caseData = await GetDataN23(caseSessionActId);

                if (caseData == null)
                    return string.Empty;

                List<WorkNotification> workNotifications = await GetSaveNotificationN23(caseSessionActId);

                if (workNotifications != null && workNotifications.Any())
                {
                    foreach (WorkNotification notification in workNotifications)
                    {
                        notification.DateCreated = GetBeginDate((notification.InitialDate ?? DateTime.Now).AddMonths(caseData.MontsFastProcess ?? 0).AddDays((caseData.WeeksFastProcess ?? 0) * 7).AddDays(caseData.DaysFastProcess ?? 0));
                        repo.Update(notification);
                    }
                }
                else
                {
                    return await SaveNotificationsForFastProcess(caseSessionActId, NomenclatureConstants.WorkNotificationType.N23, false);
                }

                if (saveChanges)
                    await repo.SaveChangesAsync();

                return string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при редакция на N23 sourceType: {7} sourceId: {caseSessionActId}");
                return ex.Message;
            }
        }

        #endregion

        #region Предприемане на действия от съдебен служител след подписване на писмо/удостоверение N24

        /// <summary>
        /// Създаване на нотификация за предприемане на действия от съдебен служител след подписване на писмо/удостоверение N24
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForN24(long documentId)
        {
            CaseInfoFastProcessVM caseData = await GetDataN24(documentId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, documentId, SourceTypeSelectVM.Document, NomenclatureConstants.WorkNotificationType.N24))
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitManualRolesFastProcess(caseData.CaseId, null);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.N24);

            DateTime _now = DateTime.Now;

            foreach (CaseLawUnitSmallVM caseLawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.Document,
                    SourceId = documentId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.N24,
                    UserId = caseLawUnit.LawUnitUserId,
                    IsUserSubstitution = caseLawUnit.IsLawUnitSubstitution,
                    FromUserId = caseLawUnit.LawUnitUserId,
                    UserCourtDepartmentId = caseLawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = _now,
                    Title = notificationTitle,
                    Description = $"{caseLawUnit.LawUnitName}{(!string.IsNullOrEmpty(caseLawUnit.DepartmentLabel) ? " (" + caseLawUnit.DepartmentLabel + ")" : string.Empty)}, по дело {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear} има подписано писмо/удостоверение, по което е необходимо да предприемете действия.",
                    NotificationKind = 2
                });
            }
            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за предприемане на действия от съдебен служител след подписване на писмо/удостоверение N24
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForN24(long documentId)
        {
            return await SaveNotificationsForFastProcess(documentId, NomenclatureConstants.WorkNotificationType.N24);
        }

        /// <summary>
        /// Гасене на нотификация за предприемане на действия от съдебен служител след подписване на писмо/удостоверение N24
        /// </summary>
        /// <param name="documentId">Идентификатор на документ</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> TurnOfNotificationsForN24(long documentId, bool saveChanges = true)
        {
            return await TurnOfNotificationsForFastProcess(SourceTypeSelectVM.Document, documentId, NomenclatureConstants.WorkNotificationType.N24, $"Сторниран е документ с идентификатор {documentId}", saveChanges);
        }

        #endregion

        #region Получено съобщение за връчване

        /// <summary>
        /// Създаване на нотификация за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <returns></returns>
        public async Task<List<WorkNotification>> GetNotificationsForReceivedMessageDeliveryFastProcess(int caseNotificationId, DateTime dateTimeEvent)
        {
            var caseData = await GetDataReceivedMessageDeliveryFastProcess(caseNotificationId);

            List<WorkNotification> notifications = [];

            if (!await ValidateForStartNotificationFastProcess(caseData, caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.ReceivedMessageDeliveryFastProcess))
                return notifications;

            List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnitManualRolesFastProcess(caseData.CaseId, null);

            string notificationTitle = await GetWorkNotificationTypeTitle(NomenclatureConstants.WorkNotificationType.ReceivedMessageDeliveryFastProcess);

            foreach (CaseLawUnitSmallVM lawUnit in caseLawUnits)
            {
                notifications.Add(new()
                {
                    SourceType = SourceTypeSelectVM.CaseNotification,
                    SourceId = caseNotificationId,
                    CourtId = caseData.CaseCourtId,
                    FromCourtId = caseData.CaseCourtId,
                    CaseId = caseData.CaseId,
                    WorkNotificationTypeId = NomenclatureConstants.WorkNotificationType.ReceivedMessageDeliveryFastProcess,
                    UserId = lawUnit.LawUnitUserId,
                    IsUserSubstitution = lawUnit.IsLawUnitSubstitution,
                    FromUserId = lawUnit.LawUnitUserId,
                    UserCourtDepartmentId = lawUnit.DepartmentId.NumberEmptyToNull(),
                    DateCreated = dateTimeEvent,
                    Title = notificationTitle,
                    Description = $"{lawUnit.LawUnitName}{(!string.IsNullOrEmpty(lawUnit.DepartmentLabel) ? " (" + lawUnit.DepartmentLabel + ")" : string.Empty)}, постъпило е съобщение за връчване по {caseData.CaseTypeLabel} {caseData.CaseRegNumber}/{caseData.CaseYear}.",
                    NotificationKind = 2
                });
            }

            return notifications;
        }

        /// <summary>
        /// Запис на нотификация за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <param name="dateTimeEvent">Дата на събитие</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<string> SaveNotificationsForReceivedMessageDeliveryFastProcess(int caseNotificationId, DateTime dateTimeEvent, bool saveChanges)
        {
            return await SaveNotificationsForFastProcess(caseNotificationId, NomenclatureConstants.WorkNotificationType.ReceivedMessageDeliveryFastProcess, saveChanges, dateTimeEvent);
        }

        /// <summary>
        /// Сторно на нотификация за получено съобщение за връчване
        /// </summary>
        /// <param name="caseNotificationId">Идентификатор на съобщението за връчване</param>
        /// <returns></returns>
        public async Task<bool> ExpiredNotificationsForReceivedMessageDeliveryFastProcess(int caseNotificationId)
        {
            return await ExpiredNotificationsForFastProcess(caseNotificationId, SourceTypeSelectVM.CaseNotification, NomenclatureConstants.WorkNotificationType.ReceivedMessageDeliveryFastProcess);
        }

        #endregion

        #region Генериране на нотификация при добавяне на служител

        /// <summary>
        /// Метод извличащ нотификации за дело
        /// </summary>
        /// <param name="caseData">Данни за дело</param>
        /// <returns></returns>
        private async Task<List<WorkNotification>> GetWorkNotifications(CaseInfoFastProcessVM caseData)
        {
            DateTime dateNow = DateTime.Now;

            return await repo.AllReadonly<WorkNotification>()
                             .Where(n => n.CaseId == caseData.CaseId)
                             .Where(n => n.DateCreated.Date >= dateNow.Date)
                             .Where(n => n.DateExpired == null || (n.DateExpired != null && n.DescriptionExpired.Contains("На служителя му е добавена дата до в делото")))
                             .ToListAsync();
        }

        /// <summary>
        /// Метод извличащ състав по дело и с премахнатите
        /// </summary>
        /// <param name="caseId">Идентификатор на дело</param>
        /// <param name="caseLawUnitId">Идентификатор на лице което се добавя</param>
        /// <returns></returns>
        private async Task<List<CaseLawUnitSmallVM>> GetCaseLawUnits(int caseId, int caseLawUnitId)
        {
            DateTime _date = DateTime.Now.AddYears(100);

            return await repo.AllReadonly<CaseLawUnit>()
                             .Where(x => x.CaseId == caseId)
                             .Where(x => x.CaseSessionId == null)
                             .Where(x => x.Id != caseLawUnitId)
                             .Select(x => new CaseLawUnitSmallVM()
                             {
                                 Id = x.Id,
                                 LawUnitId = x.LawUnitId,
                                 LawUnitUserId = x.LawUnitUserId,
                                 LawUnitName = x.LawUnit.FullName,
                                 JudgeRoleId = x.JudgeRoleId,
                                 DepartmentLabel = (x.CourtDepartmentId != null) ? " " + x.CourtDepartment.Label : string.Empty,
                                 DepartmentId = x.CourtDepartmentId,
                                 DateFrom = x.DateFrom,
                                 DateTo = x.DateTo ?? _date,
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Метод връщащ списък с данни на потребители на състава, които са от същият тип, като лицето което се добавя
        /// </summary>
        /// <param name="caseLawUnits">Списък със състав по дело</param>
        /// <param name="lawUnitAdd">Данни за лицето което се добавя в делото</param>
        /// <returns></returns>
        private List<UserDataVM> GetUserLawUnitId(List<CaseLawUnitSmallVM> caseLawUnits, CaseLawUnitSmallVM lawUnitAdd)
        {
            bool isManualRoles = NomenclatureConstants.JudgeRole.ManualRoles.Contains(lawUnitAdd.JudgeRoleId);

            if (!isManualRoles)
                return caseLawUnits.Where(x => !NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)).OrderByDescending(x => x.DateTo).Select(x => new UserDataVM() { LawUnitUserId = x.LawUnitUserId, LawUnitName = x.LawUnitName }).ToList();
            else
                return caseLawUnits.Where(x => NomenclatureConstants.JudgeRole.ManualRoles.Contains(x.JudgeRoleId)).OrderByDescending(x => x.DateTo).Select(x => new UserDataVM() { LawUnitUserId = x.LawUnitUserId, LawUnitName = x.LawUnitName }).ToList();
        }

        /// <summary>
        /// Метод връщащ нотификации от подобено лице в състава
        /// </summary>
        /// <param name="notifications">Списък с нотификации към дело</param>
        /// <param name="userIds">Идентификатор на потребители на съдебен състав</param>
        /// <returns></returns>
        private List<WorkNotification> GetWorkNotificationsFromLawUnit(List<WorkNotification> notifications, List<UserDataVM> userIds)
        {
            List<WorkNotification> workNotificationsSave = new();

            foreach (UserDataVM userId in userIds)
            {
                workNotificationsSave.AddRange(notifications.Where(x => x.UserId == userId.LawUnitUserId));
                if (workNotificationsSave.Any())
                    return workNotificationsSave;
            }

            return workNotificationsSave;
        }

        /// <summary>
        /// Метод подготвящ нотификации за запис
        /// </summary>
        /// <param name="notifications">Нотификации за потребител</param>
        /// <param name="lawUnitAdd"></param>
        /// <param name="userIds"></param>
        private void SetFieldsForSaveWorkNotificationsForSave(List<WorkNotification> notifications, CaseLawUnitSmallVM lawUnitAdd, List<UserDataVM> userIds)
        {
            string _name = userIds.Where(x => x.LawUnitUserId == notifications[0].UserId).Select(x => x.LawUnitName).FirstOrDefault();

            foreach (WorkNotification notification in notifications)
            {
                notification.Id = 0;
                notification.UserId = lawUnitAdd.LawUnitUserId;
                notification.Description = notification.Description.Replace(_name, lawUnitAdd.LawUnitName);
                notification.DateTurnOff = !string.IsNullOrEmpty(notification.DescriptionTurnOff) ? notification.DateTurnOff : null;
                notification.DateRead = null;
                notification.DateExpired = null;
                notification.DescriptionExpired = null;
            }
        }

        /// <summary>
        /// Генериране на нотификация при добавяне на служител
        /// </summary>
        /// <param name="caseLawUnitId">Идентификатор на служител</param>
        /// <returns></returns>
        public async Task<bool> CreateNotificationsAddCaseLawUnit(int caseLawUnitId)
        {
            try
            {
                // Данни за делото
                CaseInfoFastProcessVM caseData = await GetDataCreateNotificationsAddCaseLawUnit(caseLawUnitId);

                if (caseData == null)
                    return true;

                // Нотификации към делото
                List<WorkNotification> notifications = await GetWorkNotifications(caseData);
                if (notifications.Count < 1)
                    return true;

                // Съдебен състав
                List<CaseLawUnitSmallVM> caseLawUnits = await GetCaseLawUnits(caseData.CaseId, caseLawUnitId);
                if (caseLawUnits.Count < 1)
                    return true;

                // Идентификатори на потребители на състава
                List<UserDataVM> userIds = GetUserLawUnitId(caseLawUnits, caseData.CaseLawUnitSmall);
                if (userIds.Count < 1)
                    return true;

                List<WorkNotification> notificationsForSave = GetWorkNotificationsFromLawUnit(notifications, userIds);
                if (notificationsForSave.Count < 1)
                    return true;

                SetFieldsForSaveWorkNotificationsForSave(notificationsForSave, caseData.CaseLawUnitSmall, userIds);

                repo.AddRange(notificationsForSave);
                await repo.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при копиране на нотификации от служител към новодобавен служител в дело с CaseLawUnitId: {caseLawUnitId}");
                return false;
            }
        }

        #endregion

        #region Гасене на нотификации за заместващ съдия по дело при промяна на дата до в заместването

        /// <summary>
        /// Гасене на нотификации за заместващ съдия по дело при промяна на дата до в заместването
        /// </summary>
        /// <param name="judgeReporterId">Идентификатор на съдия</param>
        /// <param name="substituteJudgeReporterId">Идентификатор на заместващ съдия</param>
        /// <param name="dateTo">Дата до на заместването</param>
        /// <param name="saveChanges">Флаг дали да има SaveChanges</param>
        /// <returns></returns>
        public async Task<bool> TurnOffNotificationForSubstituteJudgeReporter(int judgeReporterId, int substituteJudgeReporterId, DateTime dateTo, bool saveChanges = true)
        {
            try
            {
                DateTime dateTime = DateTime.Now;

                List<WorkNotification> workNotifications = await repo.All<WorkNotification>()
                                                                     .Where(x => x.NotificationKind == 2)
                                                                     .Where(x => x.User.LawUnitId == substituteJudgeReporterId)
                                                                     .Where(x => x.DateCreated >= dateTo)
                                                                     .Where(x => (x.IsUserSubstitution ?? false))
                                                                     .Where(x => x.Case.CaseLawUnits.Any(l => l.DateFrom <= dateTime &&
                                                                                                              (l.DateTo ?? dateTime) >= dateTime &&
                                                                                                              l.CaseSessionId == null &&
                                                                                                              l.LawUnitId == judgeReporterId &&
                                                                                                              NomenclatureConstants.JudgeRole.JudgeRolesActiveList.Contains(l.JudgeRoleId)))
                                                                     .ToListAsync();

                if (workNotifications != null && workNotifications.Count > 0)
                {
                    workNotifications.ForEach(x => { x.DateTurnOff = dateTime; x.DescriptionTurnOff = "Автоматично гасена при смяна на дата до на заместване"; });
                    
                    if (saveChanges)
                        await repo.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при гасене на нотификации при промяна на дата на заместване на съдия. Съдия в дело: {judgeReporterId}, заместващ съдия: {substituteJudgeReporterId}");
                return false;
            }
        }

        #endregion

        #endregion
    }
}
