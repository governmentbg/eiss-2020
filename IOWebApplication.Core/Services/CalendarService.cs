using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class CalendarService : BaseService, ICalendarService
    {
        private readonly IUrlHelper urlHelper;
        private readonly IWorkTaskService taskService;
        //private readonly ICaseSessionService caseSessionService;
        public CalendarService(
            ILogger<CalendarService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            IWorkTaskService _taskService,
            //  ICaseSessionService _caseSessionService,
            IUrlHelper _url)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            urlHelper = _url;
            taskService = _taskService;
            //caseSessionService = _caseSessionService;
        }

        /// <summary>
        /// Извличане на данни за неприключили задачи със зададен краен срок и данни за заседания
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IEnumerable<CalendarVM> SelectByPerson(DateTime start, DateTime end, int caseMode = 0)
        {
            List<CalendarVM> result = new List<CalendarVM>();

            //Зареждане на неприключили задачи със зададен краен срок
            var currentTasks = repo.AllReadonly<WorkTask>()
                                    .Include(x => x.TaskType)
                                    .Include(x => x.UserCreated)
                                    .ThenInclude(x => x.LawUnit)
                                    .Where(x => x.UserId == userContext.UserId && x.DateEnd >= start && x.DateEnd <= end && x.CourtId == userContext.CourtId)
                                    .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                    .Select(x => new CalendarVM
                                    {
                                        id = x.Id,
                                        title = "Задача:" + x.TaskType.Label,
                                        start = x.DateEnd.Value,
                                        allDay = true,
                                        color = "#367fa9",
                                        SourceType = x.SourceType,
                                        SourceId = x.SourceId,
                                        pop_title = "Задача",
                                        pop_content = $"{x.TaskType.Label}, възложена от {x.UserCreated.LawUnit.FullName} на {x.DateCreated:dd.MM.yyyy HH:mm}"
                                    }).ToList();

            foreach (var item in currentTasks)
            {
                item.url = taskService.GetTaskObjectUrl(item.SourceType, item.SourceId);
                if (item.start <= DateTime.Now.ForceEndDate())
                {
                    item.color = "#a94442";
                }
                else
                {
                    if (item.start <= DateTime.Now.AddDays(2).ForceEndDate())
                    {
                        item.color = "#a33c78";
                    }
                }
            }

            result.AddRange(currentTasks);

            var lawUnitId = repo.GetPropById<ApplicationUser, int>(x => x.Id == userContext.UserId, x => x.LawUnitId);

            Expression<Func<CaseLawUnit, bool>> filterCaseMode = x => true;
            if (caseMode == 2)
            {
                filterCaseMode = x => (x.Case.IsFastProcess ?? false) == true;
            }

            //Зареждане на насрочени заседания
            var sessions = repo.AllReadonly<CaseLawUnit>()
                                    .Where(x => x.Case.CourtId == userContext.CourtId)
                                    .Where(x => x.LawUnitId == lawUnitId && x.CaseSessionId > 0)
                                    .Where(x => (x.DateTo ?? x.CaseSession.DateFrom.AddYears(100)) >= x.CaseSession.DateFrom)
                                    .Where(x => x.CaseSession.DateFrom >= start && x.CaseSession.DateFrom <= end)
                                    .Where(x => x.CaseSession.DateExpired == null)
                                    .Where(x => (x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno || x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Provedeno))
                                    .Where(filterCaseMode)
                                    .Select(x => new CalendarVM
                                    {
                                        id = x.CaseSessionId,
                                        title = x.CaseSession.SessionType.Label,
                                        start = x.CaseSession.DateFrom,
                                        end = x.CaseSession.DateTo,
                                        color = (x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno ? "#00c0ef" : "#c3c6c7"),
                                        pop_content = $"{x.Case.CaseType.Code} {x.Case.ShortNumber}/{x.Case.RegDate.ToString("yyyy")} " + ((x.CaseSession.CourtHall != null) ? $"; {x.CaseSession.CourtHall.Name} {x.CaseSession.CourtHall.Location}" : ""),
                                        pop_title = "Дело"
                                    }).ToList();
            foreach (var item in sessions)
            {
                item.url = urlHelper.Action("Preview", "CaseSession", new { id = item.id });
            }
            result.AddRange(sessions);


            if (caseMode == 2)
            {
                //Зареждане на известия
                DateTime dateNow = DateTime.Now.ForceEndDate();

                var notifications = repo.AllReadonly<WorkNotification>()
                                        .Where(x => x.CourtId == userContext.CourtId)
                                        .Where(x => x.UserId == userContext.UserId)
                                        .Where(x => x.NotificationKind == NomenclatureConstants.NotificationKinds.FastProcess)
                                        .Where(x => x.DateCreated >= start && x.DateCreated <= dateNow)
                                        .Where(x => x.DateExpired == null)
                                        .Where(x => x.DateTurnOff == null)
                                        .Select(x => new CalendarVM
                                        {
                                            id = x.Id,
                                            title = x.Title,
                                            start = x.DateCreated,
                                            end = x.DateCreated,
                                            color = x.WorkNotificationType.CalendarColor != null ? x.WorkNotificationType.CalendarColor : "#f39c12",
                                            pop_content = x.Description,
                                            pop_title = x.Title
                                        }).ToList();
                
                foreach (var item in notifications)
                {
                    item.url = urlHelper.Action("Index", "WorkNotification", new { id = item.id });
                }
                result.AddRange(notifications);

            }

            var holidays = repo.AllReadonly<WorkingDay>()
                                .Where(x => x.DayTypeId == CommonContants.WorkingDays.NotWorkDay)
                                .Where(x => x.Day >= start && x.Day <= end)
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

        ///// <summary>
        ///// Извличане на данни за заетост на зали
        ///// </summary>
        ///// <param name="CourtHallId"></param>
        ///// <param name="start"></param>
        ///// <param name="end"></param>
        ///// <returns></returns>
        //public IEnumerable<CalendarVM> SelectSessionHallUse(int CourtHallId, DateTime start, DateTime end)
        //{
        //    List<CalendarVM> result = new List<CalendarVM>();
        //    var caseSessionHallUseVMs = caseSessionService.CaseSessionHallUse_Select(userContext.CourtId, CourtHallId, start, end, null);
        //    foreach (var caseSessionHallUse in caseSessionHallUseVMs)
        //    {
        //        var calendarVM = new CalendarVM()
        //        {
        //            id = caseSessionHallUse.CourtHallId,
        //            title = caseSessionHallUse.CourtHallName,
        //            start = caseSessionHallUse.DateFrom,
        //            end = caseSessionHallUse.DateTo,
        //            pop_content = $"{caseSessionHallUse.CaseName} {caseSessionHallUse.SessionLabel}",
        //            pop_title = "Дело"
        //        };

        //        result.Add(calendarVM);
        //    }

        //    return result;
        //}
    }
}
