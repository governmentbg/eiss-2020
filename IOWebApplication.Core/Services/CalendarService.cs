using IOWebApplication.Core.Contracts;
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
using System.Linq.Expressions;

namespace IOWebApplication.Core.Services
{
    public class CalendarService : BaseService, ICalendarService
    {
        private readonly IUrlHelper urlHelper;
        private readonly IWorkTaskService taskService;
        private readonly ICaseSessionService caseSessionService;
        public CalendarService(
            ILogger<DocumentService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            IWorkTaskService _taskService,
            ICaseSessionService _caseSessionService,
            IUrlHelper _url)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            urlHelper = _url;
            taskService = _taskService;
            caseSessionService = _caseSessionService;
        }

        /// <summary>
        /// Извличане на данни за неприключили задачи със зададен краен срок и данни за заседания
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IEnumerable<CalendarVM> SelectByPerson(DateTime start, DateTime end)
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
                                        pop_content =$"{x.TaskType.Label}, възложена от {x.UserCreated.LawUnit.FullName} на {x.DateCreated:dd.MM.yyyy HH:mm}"
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

            var lawUnitId = repo.GetById<ApplicationUser>(userContext.UserId)?.LawUnitId;
            //Зареждане на насрочени заседания
            var sessions = repo.AllReadonly<CaseLawUnit>()
                                    .Include(x => x.Case)
                                    .ThenInclude(x => x.CaseType)
                                    .Include(x => x.CaseSession)
                                    .ThenInclude(x => x.SessionType)
                                    .Include(x => x.CaseSession)
                                    .ThenInclude(x => x.CourtHall)
                                    .Where(x => x.Case.CourtId == userContext.CourtId)
                                    .Where(x => x.LawUnitId == lawUnitId && x.CaseSessionId > 0)
                                    .Where(x => (x.DateTo ?? x.CaseSession.DateFrom.AddYears(100)) >= x.CaseSession.DateFrom)
                                    .Where(x => x.CaseSession.DateFrom >= start && x.CaseSession.DateFrom <= end)
                                    .Where(x => x.CaseSession.DateExpired == null)
                                    .Where(x => x.CaseSession.SessionStateId == NomenclatureConstants.SessionState.Nasrocheno)
                                    .Select(x => new CalendarVM
                                    {
                                        id = x.CaseSessionId,
                                        title = x.CaseSession.SessionType.Label,
                                        start = x.CaseSession.DateFrom,
                                        end = x.CaseSession.DateTo,
                                        color = "#00c0ef",
                                        pop_content = $"{x.Case.CaseType.Code} {x.Case.RegNumber} " + ((x.CaseSession.CourtHall != null) ? $"; {x.CaseSession.CourtHall.Name} {x.CaseSession.CourtHall.Location}" : ""),
                                        pop_title = "Дело"
                                    }).ToList();
            foreach (var item in sessions)
            {
                item.url = urlHelper.Action("Preview", "CaseSession", new { id = item.id });
            }
            result.AddRange(sessions);
            return result;
        }

        /// <summary>
        /// Извличане на данни за заетост на зали
        /// </summary>
        /// <param name="CourtHallId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public IEnumerable<CalendarVM> SelectSessionHallUse(int CourtHallId, DateTime start, DateTime end)
        {
            List<CalendarVM> result = new List<CalendarVM>();
            var caseSessionHallUseVMs = caseSessionService.CaseSessionHallUse_Select(userContext.CourtId, CourtHallId, start, end, null);
            foreach (var caseSessionHallUse in caseSessionHallUseVMs)
            {
                var calendarVM = new CalendarVM()
                {
                    id = caseSessionHallUse.CourtHallId,
                    title = caseSessionHallUse.CourtHallName,
                    start = caseSessionHallUse.DateFrom,
                    end = caseSessionHallUse.DateTo,
                    pop_content = $"{caseSessionHallUse.CaseName} {caseSessionHallUse.SessionLabel}",
                    pop_title = "Дело"
                };

                result.Add(calendarVM);
            }

            return result;
        }
    }
}
