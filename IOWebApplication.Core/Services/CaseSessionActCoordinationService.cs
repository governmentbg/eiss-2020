using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Models.ViewModels;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOWebApplication.Core.Services
{
    public class CaseSessionActCoordinationService : BaseService, ICaseSessionActCoordinationService
    {
        public CaseSessionActCoordinationService(
        ILogger<CaseSessionActCoordinationService> _logger,
        IRepository _repo,
        IUserContext _userContext)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
        }

        /// <summary>
        /// Извличане на данни за Съгласуване на актове
        /// </summary>
        /// <param name="CaseSessionActId"></param>
        /// <returns></returns>
        public IQueryable<CaseSessionActCoordinationVM> CaseSessionActCoordination_Select(int CaseSessionActId)
        {
            return repo.AllReadonly<CaseSessionActCoordination>()
                .Include(x => x.CaseSessionAct)
                .ThenInclude(x => x.ActType)
                .Include(x => x.CaseLawUnit)
                .ThenInclude(x => x.LawUnit)
                .Include(x => x.CaseLawUnit)
                .ThenInclude(x => x.JudgeRole)
                .Include(x => x.ActCoordinationType)
                .Where(x => x.CaseSessionActId == CaseSessionActId)
                .Select(x => new CaseSessionActCoordinationVM()
                {
                    Id = x.Id,
                    CaseSessionActId = x.CaseSessionActId,
                    LawUnitId = x.CaseLawUnit.LawUnitId,
                    CaseLawUnitName = (x.CaseLawUnit.LawUnit != null) ? x.CaseLawUnit.LawUnit.FullName : string.Empty,
                    ActCoordinationTypeLabel = (x.ActCoordinationType != null) ? x.ActCoordinationType.Label : string.Empty,
                    JudgeRoleLabel = (x.CaseLawUnit.JudgeRole != null) ? x.CaseLawUnit.JudgeRole.Label : string.Empty,
                    Content = x.Content,
                    ActCoordinationTypeId = x.ActCoordinationTypeId,
                    ActTypeName = x.CaseSessionAct.ActType.Label,
                    ActNumber = x.CaseSessionAct.RegNumber,
                    ActDate = x.CaseSessionAct.ActDate,
                    CanUpdate = x.CaseLawUnit.LawUnitId == userContext.LawUnitId
                }).AsQueryable();
        }

        /// <summary>
        /// Запис на Съгласуване на актове
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CaseSessionActCoordination_SaveData(CaseSessionActCoordination model)
        {
            try
            {
                if (model.Id > 0)
                {
                    //Update
                    var saved = repo.GetById<CaseSessionActCoordination>(model.Id);
                    saved.Content = model.Content;
                    saved.ActCoordinationTypeId = model.ActCoordinationTypeId;
                    saved.DateWrt = DateTime.Now;
                    saved.UserId = userContext.UserId;

                    repo.Update(saved);

                    var coordinationTasks = repo.AllReadonly<WorkTask>()
                                                    .Where(x => x.SourceType == SourceTypeSelectVM.CaseSessionAct && x.SourceId == model.CaseSessionActId)
                                                    .Where(x => x.TaskTypeId == WorkTaskConstants.Types.CaseSessionAct_Coordinate)
                                                    .Where(x => WorkTaskConstants.States.NotFinished.Contains(x.TaskStateId))
                                                    .Where(x => x.UserId == userContext.UserId)
                                                    .ToList();
                    foreach (var _task in coordinationTasks)
                    {
                        _task.TaskStateId = WorkTaskConstants.States.Completed;
                        _task.DateCompleted = DateTime.Now;
                        repo.Update(_task);
                    }
                    repo.SaveChanges();
                    return true;
                }
                else
                {
                    model.DateWrt = DateTime.Now;
                    model.UserId = userContext.UserId;
                    repo.Add<CaseSessionActCoordination>(model);
                    repo.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при запис на съгласуване Id={ model.Id }");
            }
            return false;
        }
    }
}
